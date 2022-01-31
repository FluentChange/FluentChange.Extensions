using FluentChange.Extensions.Azure.Functions.Helper;
using FluentChange.Extensions.Common.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using SystemNet = System.Net;

namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public class BuilderConfig
    {
        public JsonSerializerSettings JsonSettings { get; set; }
        public ILogger Logger { get; set; }
    }

    public class SingleBuilder
    {
        private readonly IServiceProvider provider;
        protected BuilderConfig config;
        public SingleBuilder(IServiceProvider provider)
        {
            this.provider = provider;
            this.config = new BuilderConfig();
        }

        public SingleBuilder Config(JsonSerializerSettings jsonSettings)
        {
            this.config.JsonSettings = jsonSettings;
            return this;
        }

        public SingleBuilder Config(ILogger logger)
        {
            this.config.Logger = logger;
            return this;
        }

        public SingleBuilder Config(JsonSerializerSettings jsonSettings, ILogger logger)
        {
            this.config.JsonSettings = jsonSettings;
            this.config.Logger = logger;
            return this;
        }

        public SingleBuilderWithApiInput<TApiInput> WithBody<TApiInput>(HttpRequest request)
        {
            var builder = new SingleBuilderWithApiInput<TApiInput>(provider, () => GetBody<TApiInput>(request), config);
            return builder;
        }
        public SingleBuilderWithApiInput<TApiInput> WithUnwrappingBody<TApiInput>(HttpRequest request)
        {
            var builder = new SingleBuilderWithApiInput<TApiInput>(provider, () => UnWrap<TApiInput>(request), config);
            return builder;
        }
        public SingleBuilderWithInput<TServiceInput> WithUnwrappingAndMapping<TApiInput, TServiceInput>(HttpRequest request)
        {
            var builder = new SingleBuilderWithApiInput<TApiInput>(provider, () => UnWrap<TApiInput>(request), config).MapInputTo<TServiceInput>();
            return builder;
        }
        public SingleBuilderWithApiInput<TInput> WithInput<TInput>(TInput input)
        {
            var builder = new SingleBuilderWithApiInput<TInput>(provider, () => Task.FromResult(input), config);
            return builder;
        }
        public SingleBuilderWithApiInput<TInput> WithInput<TInput>(Func<TInput> inputFunc)
        {
            var builder = new SingleBuilderWithApiInput<TInput>(provider, () => Task.FromResult(inputFunc.Invoke()), config);
            return builder;
        }
        public SingleBuilderWithApiInput<TInput> WithInput<TInput>(Func<Task<TInput>> inputFunc)
        {
            var builder = new SingleBuilderWithApiInput<TInput>(provider, inputFunc, config);
            return builder;
        }

        protected async Task<TInput> UnWrap<TInput>(HttpRequest req)
        {
            var entityWrapped = await GetBody<SingleRequest<TInput>>(req);
            return entityWrapped.Data;
        }

        protected async Task<TInput> GetBody<TInput>(HttpRequest req)
        {
            return await RequestHelper.GetRequestData<TInput>(req, config.JsonSettings);
        }

        public SingleBuilderWithoutInputAndService<TService> Use<TService>()
        {
            return new SingleBuilderWithoutInputAndService<TService>(provider, config);
        }
    }


    public class SingleBuilderWithInput<TServiceInput>
    {
        private readonly IServiceProvider provider;
        private readonly Func<Task<TServiceInput>> inputFunc;
        private readonly BuilderConfig config;
        public SingleBuilderWithInput(IServiceProvider provider, Func<Task<TServiceInput>> inputFunc, BuilderConfig config)
        {
            this.provider = provider;
            this.inputFunc = inputFunc;
            this.config = config;
        }

        public SingleBuilderWithInputAndService<TServiceInput, TService> Use<TService>()
        {
            return new SingleBuilderWithInputAndService<TServiceInput, TService>(provider, inputFunc, config);
        }
    }

    public class SingleBuilderWithApiInput<TApiInput>
    {
        private readonly IServiceProvider provider;
        private readonly Func<Task<TApiInput>> inputFunc;
        private readonly BuilderConfig config;
        public SingleBuilderWithApiInput(IServiceProvider provider, Func<Task<TApiInput>> inputFunc, BuilderConfig config)
        {
            this.provider = provider;
            this.inputFunc = inputFunc;
            this.config = config;
        }

        public SingleBuilderWithInput<TServiceInput> MapInputTo<TServiceInput>()
        {
            return new SingleBuilderWithInput<TServiceInput>(provider, () => MapTo<TServiceInput>(inputFunc), config);
        }

        private async Task<TServiceInput> MapTo<TServiceInput>(Func<Task<TApiInput>> inputFunc)
        {
            var mapper = provider.GetService<IEntityMapper>();
            if (mapper == null) throw new Exception("Mapper is missing");
            var apiInput = await inputFunc.Invoke();
            var entityMapped = mapper.MapTo<TServiceInput>(apiInput);
            return entityMapped;
        }

        public SingleBuilderWithInputAndService<TApiInput, TService> Use<TService>()
        {
            return new SingleBuilderWithInputAndService<TApiInput, TService>(provider, inputFunc, config);
        }
    }





    public class SingleBuilderWithoutInputAndService<TService>
    {
        private readonly IServiceProvider provider;
        private readonly BuilderConfig config;
        public SingleBuilderWithoutInputAndService(IServiceProvider provider, BuilderConfig config)
        {
            this.provider = provider;
            this.config = config;
        }

        public SingleBuilderWithoutInputAndServiceUse<TServiceOutput, TService> Execute<TServiceOutput>(Func<TService, Func<Task<TServiceOutput>>> update)
        {
            return new SingleBuilderWithoutInputAndServiceUse<TServiceOutput, TService>(provider, update, config);
        }
        public SingleBuilderWithoutInputAndServiceUse<TServiceOutput, TService> Execute<TServiceOutput>(Func<TService, Func<TServiceOutput>> update)
        {
            return new SingleBuilderWithoutInputAndServiceUse<TServiceOutput, TService>(provider, (service) =>
            {
                var result = update.Invoke(service).Invoke();
                return () => Task.FromResult(result);
            }
            , config);
        }

    }

    public class SingleBuilderWithInputAndService<TServiceInput, TService>
    {
        private readonly IServiceProvider provider;
        private readonly Func<Task<TServiceInput>> inputFunc;
        private readonly BuilderConfig config;
        public SingleBuilderWithInputAndService(IServiceProvider provider, Func<Task<TServiceInput>> inputFunc, BuilderConfig config)
        {
            this.provider = provider;
            this.inputFunc = inputFunc;
            this.config = config;
        }

        public SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService> Execute<TServiceOutput>(Func<TService, Func<TServiceInput, Task<TServiceOutput>>> update)
        {
            return new SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService>(provider, inputFunc, update, config);
        }

        public SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService> Execute<TServiceOutput>(Func<TService, TServiceInput, TServiceOutput> update)
        {
            return new SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService>(provider, inputFunc, (service) =>
            {
                return (input) =>
                {
                    var result = update.Invoke(service, input);
                    return Task.FromResult(result);
                };
            }
            , config);
        }

        public SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService> Execute<TServiceOutput>(Func<TService, Func<TServiceInput, TServiceOutput>> update)
        {
            return new SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService>(provider, inputFunc, (service) =>
            {
                return (input) => Task.FromResult(update.Invoke(service).Invoke(input));
            }, config);
        }
        public SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService> Execute<TServiceOutput>(Func<TService, TServiceInput, Task<TServiceOutput>> update)
        {
            return new SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService>(provider, inputFunc, (service) =>
             {

                 return (input) =>
                 {
                     var result = update.Invoke(service, input);
                     return result;
                 };
             }
            , config);
        }

    }


    public class SingleBuilderWithoutInputAndServiceUse<TServiceOutput, TService>
    {
        private readonly IServiceProvider provider;
        private readonly Func<TService, Func<Task<TServiceOutput>>> serviceFunc;
        private readonly BuilderConfig config;
        public SingleBuilderWithoutInputAndServiceUse(IServiceProvider provider,
            Func<TService, Func<Task<TServiceOutput>>> serviceFunc,
            BuilderConfig config)
        {
            this.provider = provider;
            this.serviceFunc = serviceFunc;
            this.config = config;
        }

        public SingleBuilderWithoutInputAndServiceAndMapperUse<TServiceOutput, TApiOutput, TService> MapTo<TApiOutput>()
        {
            return new SingleBuilderWithoutInputAndServiceAndMapperUse<TServiceOutput, TApiOutput, TService>(provider, serviceFunc, config);
        }


        public Executor<TServiceOutput, TServiceOutput, TService> Wrap()
        {
            return new Executor<TServiceOutput, TServiceOutput, TService>(provider, serviceFunc, true, config);
        }

        public async Task<HttpResponseMessage> Handle()
        {
            var exe = new Executor<TServiceOutput, TServiceOutput, TService>(provider, serviceFunc, false, config);
            return await exe.Respond();
        }
    }


    public class SingleBuilderWithInputAndServiceUse<TServiceInput, TServiceOutput, TService>
    {
        private readonly IServiceProvider provider;
        private readonly Func<Task<TServiceInput>> inputFunc;
        private readonly Func<TService, Func<TServiceInput, Task<TServiceOutput>>> serviceFunc;
        private readonly BuilderConfig config;
        public SingleBuilderWithInputAndServiceUse(IServiceProvider provider,
            Func<Task<TServiceInput>> inputFunc,
            Func<TService, Func<TServiceInput, Task<TServiceOutput>>> serviceFunc,
            BuilderConfig config)
        {
            this.provider = provider;
            this.inputFunc = inputFunc;
            this.serviceFunc = serviceFunc;
            this.config = config;
        }

        public SingleBuilderWithInputAndServiceAndMapperUse<TServiceInput, TServiceOutput, TApiOutput, TService> MapTo<TApiOutput>()
        {
            return new SingleBuilderWithInputAndServiceAndMapperUse<TServiceInput, TServiceOutput, TApiOutput, TService>(provider, inputFunc, serviceFunc, config);
        }


        public InputExecutor<TServiceInput, TServiceOutput, TServiceOutput, TService> Wrap()
        {
            return new InputExecutor<TServiceInput, TServiceOutput, TServiceOutput, TService>(provider, inputFunc, serviceFunc, true, config);
        }

        public async Task<HttpResponseMessage> Handle()
        {
            var exe = new InputExecutor<TServiceInput, TServiceOutput, TServiceOutput, TService>(provider, inputFunc, serviceFunc, false, config);
            return await exe.Respond();
        }
    }


    public class SingleBuilderWithoutInputAndServiceAndMapperUse<TServiceOutput, TApiOutput, TService>
    {
        private readonly IServiceProvider provider;
        private readonly Func<TService, Func<Task<TServiceOutput>>> serviceFunc;
        private readonly BuilderConfig config;
        public SingleBuilderWithoutInputAndServiceAndMapperUse(IServiceProvider provider,
            Func<TService, Func<Task<TServiceOutput>>> serviceFunc, BuilderConfig config)
        {
            this.provider = provider;
            this.serviceFunc = serviceFunc;
            this.config = config;
        }

        public Executor<TServiceOutput, TApiOutput, TService> WrapOutput()
        {
            return new Executor<TServiceOutput, TApiOutput, TService>(provider, serviceFunc, true, config);
        }

        public async Task<HttpResponseMessage> Respond()
        {
            var exe = new Executor<TServiceOutput, TApiOutput, TService>(provider, serviceFunc, false, config);
            return await exe.Respond();
        }
    }

    public class SingleBuilderWithInputAndServiceAndMapperUse<TServiceInput, TServiceOutput, TApiOutput, TService>
    {
        private readonly IServiceProvider provider;
        private readonly Func<Task<TServiceInput>> inputFunc;
        private readonly Func<TService, Func<TServiceInput, Task<TServiceOutput>>> serviceFunc;
        private readonly BuilderConfig config;
        public SingleBuilderWithInputAndServiceAndMapperUse(IServiceProvider provider,
            Func<Task<TServiceInput>> inputFunc,
            Func<TService, Func<TServiceInput, Task<TServiceOutput>>> serviceFunc, BuilderConfig config)
        {
            this.provider = provider;
            this.inputFunc = inputFunc;
            this.serviceFunc = serviceFunc;
            this.config = config;
        }

        public InputExecutor<TServiceInput, TServiceOutput, TApiOutput, TService> WrapOutput()
        {
            return new InputExecutor<TServiceInput, TServiceOutput, TApiOutput, TService>(provider, inputFunc, serviceFunc, true, config);
        }

        public async Task<HttpResponseMessage> Respond()
        {
            var exe = new InputExecutor<TServiceInput, TServiceOutput, TApiOutput, TService>(provider, inputFunc, serviceFunc, false, config);
            return await exe.Respond();
        }
    }



    public class Executor<TServiceOutput, TApiOutput, TService>
    {
        private readonly IServiceProvider provider;
        protected bool wrapResponse;
        protected readonly bool usesOutputMapping;
        private readonly IEntityMapper mapper;
        protected BuilderConfig config;
        private readonly Func<TService, Func<Task<TServiceOutput>>> serviceFuncAsync;
        private readonly ILogger log;

        public Executor(IServiceProvider provider, Func<TService, Func<Task<TServiceOutput>>> serviceFunc,
            bool wrapResponse, BuilderConfig config)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            this.provider = provider;
            this.mapper = GetMapperService();
            this.log = provider.GetService<ILogger>();
            this.wrapResponse = wrapResponse;
            this.usesOutputMapping = !typeof(TServiceOutput).Equals(typeof(TApiOutput));
            this.serviceFuncAsync = serviceFunc;
            this.config = config;
        }

        private IEntityMapper GetMapperService()
        {
            var mapper = provider.GetService<IEntityMapper>();
            if (mapper == null) throw new Exception("Mapper is missing");

            return mapper;
        }

        private HttpResponseMessage RespondError(Exception ex)
        {
            if (config.Logger != null && wrapResponse) config.Logger.LogError(ex, "Execution error in handler");
            var errorInfo = new Common.Models.ErrorInfo() { Message = ex.Message, FullMessage = ex.ToString() };
            if (wrapResponse)
            {
                var response = new Response();
                response.Errors.Add(errorInfo);
                return ResponseHelper.CreateJsonResponse(response, SystemNet.HttpStatusCode.InternalServerError, config.JsonSettings);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(errorInfo, SystemNet.HttpStatusCode.InternalServerError, config.JsonSettings);
            }
        }
        private HttpResponseMessage RespondEmpty()
        {
            if (wrapResponse)
            {
                var response = new Response();
                return ResponseHelper.CreateJsonResponse(response, config.JsonSettings);
            }
            else
            {
                return ResponseHelper.CreateJsonResponse(null, config.JsonSettings);
            }
        }
        private HttpResponseMessage RespondSingle(TServiceOutput result)
        {

            if (config.Logger != null && usesOutputMapping) config.Logger.LogInformation("Map output to " + typeof(TApiOutput).Name);
            if (config.Logger != null && wrapResponse) config.Logger.LogInformation("Wrap response");

            if (wrapResponse)
            {
                if (usesOutputMapping)
                {
                    var mappedResult = mapper.MapTo<TApiOutput>(result);
                    var response = new NewResponse<TApiOutput>();
                    response.Data = mappedResult;
                    return ResponseHelper.CreateJsonResponse(response, config.JsonSettings);
                }
                else
                {
                    var response = new NewResponse<TServiceOutput>();
                    response.Data = result;
                    return ResponseHelper.CreateJsonResponse(response, config.JsonSettings);
                }

            }
            else
            {
                if (usesOutputMapping)
                {
                    var mappedResult = mapper.MapTo<TApiOutput>(result);
                    return ResponseHelper.CreateJsonResponse(mappedResult, config.JsonSettings);
                }
                else
                {
                    return ResponseHelper.CreateJsonResponse(result, config.JsonSettings);
                }
            }
        }

        public async Task<HttpResponseMessage> Respond()
        {


            try
            {
                var service = provider.GetService<TService>();

                if (serviceFuncAsync == null) throw new NotImplementedException();
                if (config.Logger != null) config.Logger.LogInformation("Service used: " + typeof(TService).Name);

                TServiceOutput serviceOutput = await serviceFuncAsync.Invoke(service).Invoke();
                if (config.Logger != null) config.Logger.LogInformation("Service output: " + typeof(TServiceOutput).FullName);

                return RespondSingle(serviceOutput);

            }
            catch (Exception ex)
            {

                return RespondError(ex);
            }
        }
    }

    public class InputExecutor<TServiceInput, TServiceOutput, TApiOutput, TService>
    {

        private readonly Func<Task<TServiceInput>> inputFunc;
        private readonly Func<TService, Func<TServiceInput, Task<TServiceOutput>>> serviceFunc;
        protected BuilderConfig config;
        private readonly Executor<TServiceOutput, TApiOutput, TService> executor;

        public InputExecutor(IServiceProvider provider, Func<Task<TServiceInput>> inputFunc, Func<TService, Func<TServiceInput, Task<TServiceOutput>>> serviceFunc,
            bool wrapResponse, BuilderConfig config)
        {
            this.inputFunc = inputFunc;
            this.serviceFunc = serviceFunc;
            this.config = config;
            executor = new Executor<TServiceOutput, TApiOutput, TService>(provider, service => Execute(service), wrapResponse, config);
        }

        public Func<Task<TServiceOutput>> Execute(TService service)
        {
            return async () =>
            {
                TServiceInput input = await inputFunc.Invoke();
                if (config.Logger != null) config.Logger.LogInformation("Service input: " + typeof(TServiceInput).FullName);

                if (input == null) throw new Exception("input was null");
                if (serviceFunc == null) throw new NotImplementedException();
                TServiceOutput serviceOutput = await serviceFunc.Invoke(service).Invoke(input);
                return serviceOutput;
            };
        }

        public async Task<HttpResponseMessage> Respond()
        {
            return await executor.Respond();
        }
    }
}
