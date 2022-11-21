using FluentChange.Extensions.Common.Models;
using FluentChange.Extensions.Common.Models.Rest;
using FluentChange.Extensions.System.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
    public abstract class AbstractWrappedApi<T> where T : class
    {
        protected readonly CompleteWrappedApi<T, Guid> internalApi;
        protected Dictionary<string, object> routeParams;
        protected readonly IRestClient rest;
        public AbstractWrappedApi(IRestClient rest, string route, Dictionary<string, object> routeParams)
        {
            this.rest = rest;
            this.routeParams = routeParams;
            this.internalApi = new CompleteWrappedApi<T, Guid>(rest, route, routeParams);
        }
    }

    public class WrappedGenericCLApi<T> : AbstractWrappedApi<T> where T : class, IEntityWithId
    {
        public WrappedGenericCLApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<DataResponse<T>> Create(T entity) => await internalApi.Create(entity);
        public async Task<DataResponse<IEnumerable<T>>> List() => await internalApi.List();
    }
    public class WrappedGenericRUWithIdApi<T> : AbstractWrappedApi<T> where T : class
    {
        public WrappedGenericRUWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        protected async Task<DataResponse<T>> Read(Guid id) => await internalApi.Read(id);
        protected async Task<DataResponse<T>> Update(Guid id, T entity) => await internalApi.Update(id, entity);
        public async Task<DataResponse<T>> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity);
    }
    public class WrappedGenericRUDWithIdApi<T> : AbstractWrappedApi<T> where T : class
    {
        public WrappedGenericRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }
        protected async Task<DataResponse<T>> Read(Guid id) => await internalApi.Read(id);
        protected async Task<DataResponse<T>> Update(Guid id, T entity) => await internalApi.Update(id, entity);
        public async Task<DataResponse<T>> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity);
        public async Task<Response> Delete(Guid id) => await internalApi.Delete(id);
        public async Task<Response> Delete<X>(X entity) where X : T, IEntityWithId => await internalApi.Delete(entity.Id);
    }
    public class WrappedGenericRUWithoutIdApi<T> : AbstractWrappedApi<T> where T : class
    {
        public WrappedGenericRUWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<DataResponse<T>> Read() => await internalApi.Read();
        public async Task<DataResponse<T>> Update(T entity) => await internalApi.Update(entity);

    }
    public class WrappedGenericRUDWithoutIdApi<T> : AbstractWrappedApi<T> where T : class
    {
        public WrappedGenericRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }
        public async Task<DataResponse<T>> Read() => await internalApi.Read();
        public async Task<DataResponse<T>> Update(T entity) => await internalApi.Update(entity);
        public async Task<Response> Delete() => await internalApi.Delete();

    }
    public class WrappedGenericCRUDWithIdApi<T> : AbstractWrappedApi<T> where T : class
    {
        public WrappedGenericCRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<DataResponse<T>> Create(T entity) => await internalApi.Create(entity);
        public async Task<DataResponse<T>> Read(Guid id) => await internalApi.Read(id);
        public async Task<DataResponse<T>> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity);
        public async Task<Response> Delete(Guid id) => await internalApi.Delete(id);
        public async Task<Response> Delete<X>(X entity) where X : T, IEntityWithId => await internalApi.Delete(entity.Id);
    }
    public class WrappedGenericCRUDWithoutIdApi<T> : AbstractWrappedApi<T> where T : class
    {
        public WrappedGenericCRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<DataResponse<T>> Create(T entity) => await internalApi.Create(entity);
        public async Task<DataResponse<T>> Read() => await internalApi.Read();
        public async Task<DataResponse<T>> Update(T entity) => await internalApi.Update(entity);
        public async Task<Response> Delete() => await internalApi.Delete();
    }
    public class WrappedGenericCRUDLWithIdApi<T> : AbstractWrappedApi<T> where T : class
    {
        public WrappedGenericCRUDLWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<DataResponse<T>> Create(T entity) => await internalApi.Create(entity);
        public async Task<DataResponse<T>> Read(Guid id) => await internalApi.Read(id);
        public async Task<DataResponse<T>> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity);
        public async Task<Response> Delete(Guid id) => await internalApi.Delete(id);
        public async Task<Response> Delete<X>(X entity) where X : T, IEntityWithId => await internalApi.Delete(entity.Id);
        public async Task<DataResponse<IEnumerable<T>>> List() => await internalApi.List();
    }



    public abstract class AbstractGenericApi<T> where T : class
    {
        protected readonly CompleteApi<T, Guid> internalApi;
        protected Dictionary<string, object> routeParams;
        protected readonly IRestClient rest;
        public AbstractGenericApi(IRestClient rest, string route, Dictionary<string, object> routeParams)
        {
            this.rest = rest;
            this.routeParams = routeParams;
            this.internalApi = new CompleteApi<T, Guid>(rest, route, routeParams);
        }
    }

    public class GenericCLApi<T> : AbstractGenericApi<T> where T : class, IEntityWithId
    {
        public GenericCLApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Create(T entity) => await internalApi.Create(entity);
        public async Task<IEnumerable<T>> List() => await internalApi.List();
    }
    public class GenericRUWithIdApi<T> : AbstractGenericApi<T> where T : class
    {
        public GenericRUWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Read(Guid id) => await internalApi.Read(id);
        public async Task<T> Update(Guid id, T entity) => await internalApi.Update(id, entity);
        public async Task<T> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity.Id, entity);
    }
    public class GenericRUDWithIdApi<T> : AbstractGenericApi<T> where T : class
    {
        public GenericRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Read(Guid id) => await internalApi.Read(id);
        public async Task<T> Update(Guid id, T entity) => await internalApi.Update(id, entity);
        public async Task<T> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity.Id, entity);
        public async Task Delete(Guid id) => await internalApi.Delete(id);
        public async Task Delete<X>(X entity) where X : T, IEntityWithId => await internalApi.Delete(entity.Id);
    }
    public class GenericRUWithoutIdApi<T> : AbstractGenericApi<T> where T : class
    {
        public GenericRUWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Read() => await internalApi.Read();
        public async Task<T> Update(T entity) => await internalApi.Update(entity);
    }
    public class GenericRUDWithoutIdApi<T> : AbstractGenericApi<T> where T : class
    {
        public GenericRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Read() => await internalApi.Read();
        public async Task<T> Update(T entity) => await internalApi.Update(entity);
        public async Task Delete() => await internalApi.Delete();

    }
    public class GenericCRUDWithIdApi<T> : AbstractGenericApi<T> where T : class, IEntityWithId
    {
        public GenericCRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }
        public async Task<T> Create(T entity) => await internalApi.Create(entity);
        public async Task<T> Read(Guid id) => await internalApi.Read(id);
        public async Task<T> Update(Guid id, T entity) => await internalApi.Update(id, entity);
        public async Task<T> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity.Id, entity);
        public async Task Delete(Guid id) => await internalApi.Delete(id);
        public async Task Delete<X>(X entity) where X : T, IEntityWithId => await internalApi.Delete(entity.Id);
    }
    public class GenericCRUDWithoutIdApi<T> : AbstractGenericApi<T> where T : class
    {
        public GenericCRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Create(T entity) => await internalApi.Create(entity);
        public async Task<T> Read() => await internalApi.Read();
        public async Task<T> Update(T entity) => await internalApi.Update(entity);
        public async Task Delete() => await internalApi.Delete();

    }
    public class GenericCRUDLWithIdApi<T> : AbstractGenericApi<T> where T : class, IEntityWithId
    {
        public GenericCRUDLWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Create(T entity) => await internalApi.Create(entity);
        public async Task<T> Read(Guid id) => await internalApi.Read(id);
        public async Task<T> Update(Guid id, T entity) => await internalApi.Update(id, entity);
        public async Task<T> Update<X>(X entity) where X : T, IEntityWithId => await internalApi.Update(entity.Id, entity);
        public async Task Delete(Guid id) => await internalApi.Delete(id);
        public async Task Delete<X>(X entity) where X : T, IEntityWithId => await internalApi.Delete(entity.Id);
        public async Task<IEnumerable<T>> List() => await internalApi.List();
    }


    public enum ExecuteMethod
    {
        GET,
        POST,
        PUT,
        DELETE

    }
    public abstract class AbstractExecuteApi<T> where T : class
    {
        protected readonly IRestClient rest;
        protected readonly string route;
        protected Dictionary<string, object> routeParams;

        public AbstractExecuteApi(IRestClient rest, string route, Dictionary<string, object> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;
        }
    }
    public class WrappedExecuteApi<T> : AbstractExecuteApi<T> where T : class, IEntityWithId
    {
        private readonly ExecuteMethod method;
        public WrappedExecuteApi(IRestClient rest, string route, Dictionary<string, object> routeParams, ExecuteMethod method) : base(rest, route, routeParams)
        {
            this.method = method;
        }

        public async Task<DataResponse<T>> ReadSingle(Guid? id = null, T data = null)
        {
            return await Execute<DataResponse<T>>(id, data);
        }

        public async Task<DataResponse<IEnumerable<T>>> ReadMutliple(Guid? id = null, T data = null)
        {
            return await Execute<DataResponse<IEnumerable<T>>>(id, data);
        }

        private async Task<R> Execute<R>(Guid? id = null, T data = null)
        {
            var paramsDic = routeParams.Copy();
            var finalroute = route;
            if (id.HasValue)
            {
                paramsDic.Add(Routes.ParamNameId, id.ToString());
                if (!finalroute.Contains(Routes.PatternId))
                {
                    finalroute = finalroute + Routes.PatternId;
                }
            }

            object requestBody = null;
            if (data != null)
            {
                if (method != ExecuteMethod.POST && method != ExecuteMethod.PUT) throw new Exception("requestbody data only for POST or PUT possible");

                var request = new SingleRequest<T>();
                request.Data = data;
                requestBody = request;
            }

            switch (method)
            {
                case ExecuteMethod.GET: return await rest.Get<R>(finalroute, paramsDic);
                case ExecuteMethod.PUT: return await rest.Put<R>(finalroute, requestBody, paramsDic);
                case ExecuteMethod.POST: return await rest.Post<R>(finalroute, requestBody, paramsDic);
                case ExecuteMethod.DELETE: return await rest.Delete<R>(finalroute, paramsDic);
                default: throw new NotImplementedException();
            }
        }


    }
}
