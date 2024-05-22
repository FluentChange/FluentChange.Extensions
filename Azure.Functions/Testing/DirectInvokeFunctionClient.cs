using System;
using System.Collections.Generic;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentChange.Extensions.Common.Rest;
using Microsoft.Extensions.Primitives;
using Microsoft.Azure.Functions.Worker;

namespace FluentChange.Extensions.Azure.Functions.Testing
{
    public class DirectInvokeFunctionClient<T> : AbstractInternalRestClient where T : FunctionsStartup
    {
        public ILogger Logger { get; private set; }
        public ServiceProvider GlobalServiceProvider { get; private set; }
        private Dictionary<string, string> headers = new Dictionary<string, string>();

        private Dictionary<string, Dictionary<string, MethodInfo>> routeMapping = new Dictionary<string, Dictionary<string, MethodInfo>>();

        public DirectInvokeFunctionClient()
        {
            var assembly = Assembly.GetAssembly(typeof(T));
            var allTypesWithFunctions = assembly.GetTypes()
                   .Where(t => t.GetMethods().Any(m => m.GetCustomAttributes(typeof(FunctionNameAttribute), false).Length > 0))
                   .ToList();



            var allMethodsWithHttpTriggers = allTypesWithFunctions
                      .SelectMany(t => t.GetMethods())
                      .Where(m => m.GetCustomAttributes(typeof(FunctionNameAttribute), false).Length > 0
                      && m.GetParameters().Any(m => m.GetCustomAttributes(typeof(HttpTriggerAttribute), false).Length > 0))
                      .ToArray();


            foreach (var method in allMethodsWithHttpTriggers)
            {
                var parameters = method.GetParameters();
                var parametersWithAtrribute = parameters.Where(p => p.GetCustomAttributes(typeof(HttpTriggerAttribute), false).Length > 0).ToList();

                var oneAndOnlyParameter = parametersWithAtrribute.Single();
                var customeAttributes = oneAndOnlyParameter.GetCustomAttributes(typeof(HttpTriggerAttribute), false);
                var attribute = (HttpTriggerAttribute)customeAttributes.Single();

                if (attribute != null)
                {
                    foreach (var httpMethod in attribute.Methods)
                    {
                        if (!routeMapping.ContainsKey(httpMethod))
                        {
                            routeMapping.Add(httpMethod, new Dictionary<string, MethodInfo>());
                        }
                        if (!routeMapping[httpMethod].ContainsKey(attribute.Route))
                        {
                            routeMapping[httpMethod].Add(attribute.Route, method);
                            if (attribute.Route.Contains("{id?}"))
                            {
                                var replacedRoute = attribute.Route.Replace("{id?}", "{id}");
                                routeMapping[httpMethod].Add(replacedRoute, method);
                            }
                        }
                    }
                }


            }

            GlobalServiceProvider = CreateNewServiceProvider(allTypesWithFunctions);
            Logger = new ListLogger();
        }

        private static ServiceProvider CreateNewServiceProvider(List<Type> functionClasses)
        {
            var services = new ServiceCollection();

            // Handle settings configuration

            var basePath = Directory.GetParent(AppContext.BaseDirectory).FullName;
            var fileName = "appsettings.json";
            var fullPath = Path.Combine(basePath, fileName);
            var configFileExist = File.Exists(fullPath);

            if (configFileExist)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile(fileName, false)
                    .Build();

                var envValues3 = configuration.AsEnumerable();
                foreach (var envValue in envValues3.Where(v => v.Key.StartsWith("Values:")))
                {
                    var variableName = envValue.Key.Replace("Values:", "");
                    Environment.SetEnvironmentVariable(variableName, envValue.Value);
                }

                // Add access to generic IConfigurationRoot
                services.AddSingleton<IConfigurationRoot>(configuration);
                services.AddSingleton<IConfiguration>(configuration);
            }

            services.AddLogging();

            var dummyHostBuilder = new DummyFunctionsHostBuilder(services);
            var functionsStartup = (T)Activator.CreateInstance(typeof(T));
            functionsStartup.Configure(dummyHostBuilder);


            foreach (var f in functionClasses)
            {
                services.AddScoped(f);
            }

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }


        protected async override Task<HttpResponseMessage> GetImplementation(string route, Dictionary<string, object> parameters)
        {
            using (var scope = GlobalServiceProvider.CreateScope())
            {
                var functionMethod = findFunctionMethod("get", route);
                var functionClassInstance = scope.ServiceProvider.GetService(functionMethod.DeclaringType);
                var functionParameters = await CreateFunctionParameters("get", route, parameters, functionMethod);

                return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClassInstance, functionParameters.ToArray());
            }
        }

        protected async override Task<HttpResponseMessage> PostImplementation(string route, object content, Dictionary<string, object> parameters = null)
        {
            using (var scope = GlobalServiceProvider.CreateScope())
            {
                var functionMethod = findFunctionMethod("post", route);
                var functionClassInstance = scope.ServiceProvider.GetService(functionMethod.DeclaringType);
                var functionParameters = await CreateFunctionParameters("post", route, parameters, functionMethod, content);

                return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClassInstance, functionParameters.ToArray());
            }
        }

        protected async override Task<HttpResponseMessage> PutImplementation(string route, object content, Dictionary<string, object> parameters = null)
        {
            using (var scope = GlobalServiceProvider.CreateScope())
            {
                var functionMethod = findFunctionMethod("put", route);
                var functionClassInstance = scope.ServiceProvider.GetService(functionMethod.DeclaringType);
                var functionParameters = await CreateFunctionParameters("put", route, parameters, functionMethod, content);

                return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClassInstance, functionParameters.ToArray());
            }
        }

        protected async override Task<HttpResponseMessage> DeleteImplementation(string route, Dictionary<string, object> parameters)
        {
            using (var scope = GlobalServiceProvider.CreateScope())
            {
                var functionMethod = findFunctionMethod("delete", route);
                var functionClass = scope.ServiceProvider.GetService(functionMethod.DeclaringType);
                var functionParameters = await CreateFunctionParameters("delete", route, parameters, functionMethod);

                return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClass, functionParameters.ToArray());
            }
        }

        private MethodInfo findFunctionMethod(string method, string route)
        {
            if (routeMapping[method].ContainsKey(route))
            {
                return routeMapping[method][route];
            }
            else
            {
                if (routeMapping[method].ContainsKey(route + RouteHelper.Id))
                {
                    return routeMapping[method][route + RouteHelper.Id];
                }
            }

            throw new Exception("function method for route " + method + " " + route + " not found");
        }

        private async Task<List<object>> CreateFunctionParameters(string method, string route, Dictionary<string, object> parameters, MethodInfo functionMethod, object content = null)
        {
            var routeValues = new Dictionary<string, string>();
            route = ReplaceParams(route, parameters, out routeValues);
            var dummyHttpRequest = await CreateHttpRequest(method, route, content);


            // ADD Routevalues
            foreach (var entry in routeValues)
            {
                dummyHttpRequest.RouteValues.Add(entry.Key, entry.Value);
            }

            var functionParameters = new List<object>();
            var onlyOneComplexClassType = false;
            foreach (var definedParam in functionMethod.GetParameters())
            {
                if (definedParam.ParameterType == typeof(HttpRequest))
                {
                    functionParameters.Add(dummyHttpRequest);
                }
                else if (definedParam.ParameterType == typeof(ILogger))
                {
                    functionParameters.Add(Logger);
                }
                else if (definedParam.ParameterType.IsValueType || definedParam.ParameterType == typeof(string))
                {
                    if (parameters.ContainsKey(definedParam.Name))
                    {
                        var parameterValue = parameters[definedParam.Name];
                        if (definedParam.ParameterType == parameterValue.GetType())
                        {
                            functionParameters.Add(parameterValue);
                        }
                        else
                        {
                            throw new Exception("paramter missmatch");
                        }

                    }
                    else
                    {
                        functionParameters.Add(null);
                    }
                }
                else
                {
                    if (onlyOneComplexClassType) throw new Exception("only one complex type as parameter binding ins possible");
                    onlyOneComplexClassType = true;
                    functionParameters.Add(content);
                }
            }

            return functionParameters;
        }

        private async Task<HttpRequest> CreateHttpRequest(string method, string route, object content = null)
        {
            var dummyHttpContext = new DefaultHttpContext();
            var dummyHttpRequest = dummyHttpContext.Request;
            dummyHttpRequest.Method = method.ToUpper();
            dummyHttpRequest.Host = new HostString("https://localhost");
            dummyHttpRequest.Path = "/" + route;
            foreach (var header in headers)
            {
                dummyHttpRequest.Headers.Add(header.Key, header.Value);
            }

            if (content != null)
            {
                var httpContent = SerializeContentIfNeeded(content);
                var stream = await httpContent.ReadAsStreamAsync();
                dummyHttpRequest.Body = stream;
                foreach (var header in httpContent.Headers)
                {
                    var sv = new StringValues(header.Value.ToArray());
                    dummyHttpRequest.Headers.Add(header.Key, sv);
                }
            }

            return dummyHttpRequest;
        }

        public override bool ExistHeader(string key)
        {
            return headers.ContainsKey(key);
        }

        public override void SetHeader(string key, string value)
        {
            if (headers.ContainsKey(key))
            {
                headers[key] = value;
            }
            else
            {
                headers.Add(key, value);
            }
        }
        public override void RemoveHeader(string key)
        {
            headers.Remove(key);
        }
    }
}
