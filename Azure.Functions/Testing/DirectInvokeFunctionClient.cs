﻿using System;
using System.Collections.Generic;
using System.Text;
using FluentChange.Extensions.Azure.Functions.CRUDL;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using FluentChange.Extensions.Common.Rest;

namespace FluentChange.Extensions.Azure.Functions.Testing
{
    public class DirectInvokeFunctionClient<T> : AbstractInternalRestClient where T : FunctionsStartup
    {
        protected ILogger logger { get; set; }
        private ServiceProvider serviceProvider { get; set; }
        private Dictionary<string, string> headers = new Dictionary<string, string>();

        private Dictionary<string, Dictionary<string, MethodInfo>> Mapping = new Dictionary<string, Dictionary<string, MethodInfo>>();

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
                //var parameterInfo = parametersWithAtrribute.SelectMany(t => t.CustomAttributes);



                var oneAndOnlyParameter = parametersWithAtrribute.Single();
                var customeAttributes = oneAndOnlyParameter.GetCustomAttributes(typeof(HttpTriggerAttribute), false);
                var attribute = (HttpTriggerAttribute)customeAttributes.Single();

                if (attribute != null)
                {
                    foreach (var httpMethod in attribute.Methods)
                    {
                        if (!Mapping.ContainsKey(httpMethod))
                        {
                            Mapping.Add(httpMethod, new Dictionary<string, MethodInfo>());
                        }
                        if (!Mapping[httpMethod].ContainsKey(attribute.Route))
                        {
                            Mapping[httpMethod].Add(attribute.Route, method);
                            if (attribute.Route.Contains("{id?}"))
                            {
                                var replacedRoute = attribute.Route.Replace("{id?}", "{id}");
                                Mapping[httpMethod].Add(replacedRoute, method);
                            }
                        }
                    }
                }


            }

            serviceProvider = CreateNewServiceProvider(allTypesWithFunctions);
            logger = new ListLogger();
        }

        private static ServiceProvider CreateNewServiceProvider(List<Type> functionClasses)
        {
            var basePath = Directory.GetParent(AppContext.BaseDirectory).FullName;
            var fileName = "appsettings.json";
            var fullPath = Path.Combine(basePath, fileName);

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

            var services = new ServiceCollection();
            // Add access to generic IConfigurationRoot
            services.AddSingleton<IConfigurationRoot>(configuration);
            services.AddSingleton<IConfiguration>(configuration);

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


        protected async override Task<HttpResponseMessage> GetImplementation(string route, Dictionary<string, string> parameters)
        {
            var functionMethod = findFunctionMethod("get", route);
            var functionClassInstance = serviceProvider.GetService(functionMethod.DeclaringType);
            var functionParameters = await CreateFunctionParameters("get", route, parameters, functionMethod);

            return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClassInstance, functionParameters.ToArray());
        }

        protected async override Task<HttpResponseMessage> PostImplementation(string route, HttpContent content, Dictionary<string, string> parameters = null)
        {
            var functionMethod = findFunctionMethod("post", route);
            var functionClassInstance = serviceProvider.GetService(functionMethod.DeclaringType);
            var functionParameters = await CreateFunctionParameters("post", route, parameters, functionMethod, content);

            return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClassInstance, functionParameters.ToArray());
        }

        protected async override Task<HttpResponseMessage> PutImplementation(string route, HttpContent content, Dictionary<string, string> parameters = null)
        {
            var functionMethod = findFunctionMethod("put", route);
            var functionClassInstance = serviceProvider.GetService(functionMethod.DeclaringType);
            var functionParameters = await CreateFunctionParameters("put", route, parameters, functionMethod, content);

            return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClassInstance, functionParameters.ToArray());
        }

        protected async override Task<HttpResponseMessage> DeleteImplementation(string route, Dictionary<string, string> parameters)
        {
            var functionMethod = findFunctionMethod("delete", route);
            var functionClass = serviceProvider.GetService(functionMethod.DeclaringType);
            var functionParameters = await CreateFunctionParameters("delete", route, parameters, functionMethod);

            return await (Task<HttpResponseMessage>)functionMethod.Invoke(functionClass, functionParameters.ToArray());
        }

        private MethodInfo findFunctionMethod(string method, string route)
        {
            if (Mapping[method].ContainsKey(route))
            {
                return Mapping[method][route];
            }
            else
            {
                if (Mapping[method].ContainsKey(route + CRUDLHelper.Id))
                {
                    return Mapping[method][route + CRUDLHelper.Id];
                }
            }

            throw new Exception("function method for route " + method + " " + route + " not found");
        }

        private async Task<List<object>> CreateFunctionParameters(string method, string route, Dictionary<string, string> parameters, MethodInfo functionMethod, HttpContent content = null)
        {
            route = ReplaceParams(route, parameters);
            var dummyHttpRequest = await CreateHttpRequest(method, route, content);

            var functionParameters = new List<object>();
            foreach (var definedParam in functionMethod.GetParameters())
            {
                if (definedParam.ParameterType == typeof(HttpRequest))
                {
                    functionParameters.Add(dummyHttpRequest);
                }
                else if (definedParam.ParameterType == typeof(ILogger))
                {
                    functionParameters.Add(logger);
                }
                else
                {
                    if (parameters.ContainsKey(definedParam.Name))
                    {
                        functionParameters.Add(parameters[definedParam.Name]);
                    }
                    else
                    {
                        functionParameters.Add(null);
                    }
                }
            }

            return functionParameters;
        }

        private async Task<HttpRequest> CreateHttpRequest(string method, string route, HttpContent content = null)
        {
            var dummyHttpContext = new DefaultHttpContext();
            var dummyHttpRequest = dummyHttpContext.Request;
            dummyHttpRequest.Method = method.ToUpper();
            dummyHttpRequest.Host = new HostString("https://localhost");
            dummyHttpRequest.Path = "/" + route;
            //dummyHttpRequest.Headers.Add("Authorization", "Secret meowmeow6789");
            foreach (var header in headers)
            {
                dummyHttpRequest.Headers.Add(header.Key, header.Value);
            }

            if (content != null)
            {
                dummyHttpRequest.Body = await content.ReadAsStreamAsync();
            }

            return dummyHttpRequest;
        }

        protected override bool ExistHeader(string key)
        {
            return headers.ContainsKey(key);
        }

        protected override void SetHeader(string key, string value)
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

        protected override void RemoveHeader(string key)
        {
            headers.Remove(key);
        }

    }
}
