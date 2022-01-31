﻿using FluentChange.Extensions.Common.Models;
using FluentChange.Extensions.Common.Models.Rest;
using FluentChange.Extensions.System.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
    public abstract class BaseAbstractApi<T> where T : class
    {
        protected readonly IRestClient rest;
        protected readonly string route;
        protected Dictionary<string, object> routeParams;

        public BaseAbstractApi(IRestClient rest, string route, Dictionary<string, object> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;
        }
    }

    public class WrappedGenericCLApi<T> : BaseAbstractApi<T> where T : class, IEntityWithId
    {
        public WrappedGenericCLApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<SingleResponse<T>> Create(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Post<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<MultiResponse<T>> List()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<MultiResponse<T>>(route, paramsDic);
        }

    }
    public class WrappedGenericCRUDWithIdApi<T> : BaseAbstractApi<T> where T : class
    {
        public WrappedGenericCRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<SingleResponse<T>> Create(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Post<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<SingleResponse<T>> Read(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            var read = rest.Get<SingleResponse<T>>(route, paramsDic);
            return await read;
        }

        public async Task<SingleResponse<T>> Update(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Put<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<Response> Delete(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Delete<Response>(route, paramsDic);
        }
    }
    public class WrappedGenericCRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : class
    {
        public WrappedGenericCRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<SingleResponse<T>> Create(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Post<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<SingleResponse<T>> Read()
        {
            var paramsDic = routeParams.Copy();
            var read = rest.Get<SingleResponse<T>>(route, paramsDic);
            return await read;
        }

        public async Task<SingleResponse<T>> Update(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Put<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<Response> Delete()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Delete<Response>(route, paramsDic);
        }
    }
    public class WrappedGenericCRUDLWithIdApi<T> : BaseAbstractApi<T> where T : class, IEntityWithId
    {
        public WrappedGenericCRUDLWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<SingleResponse<T>> Create(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Post<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<SingleResponse<T>> Read(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            var read = rest.Get<SingleResponse<T>>(route + Routes.PatternId, paramsDic);
            return await read;
        }

        public async Task<SingleResponse<T>> Update(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, entity.Id.ToString());

            return await rest.Put<SingleResponse<T>>(route + Routes.PatternId, request, paramsDic);
        }

        public async Task<Response> Delete(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            return await rest.Delete<Response>(route + Routes.PatternId, paramsDic);
        }

        public async Task<MultiResponse<T>> List()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<MultiResponse<T>>(route, paramsDic);
        }
    }
    public class WrappedGenericRUDWithIdApi<T> : BaseAbstractApi<T> where T : class
    {
        public WrappedGenericRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<SingleResponse<T>> Read(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            var read = rest.Get<SingleResponse<T>>(route, paramsDic);
            return await read;
        }

        public async Task<SingleResponse<T>> Update(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Put<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<Response> Delete(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            return await rest.Delete<Response>(route + Routes.PatternId, paramsDic);
        }
    }
    public class WrappedGenericRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : class
    {
        public WrappedGenericRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<SingleResponse<T>> Read()
        {
            var paramsDic = routeParams.Copy();
            var read = rest.Get<SingleResponse<T>>(route, paramsDic);
            return await read;
        }

        public async Task<SingleResponse<T>> Update(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Put<SingleResponse<T>>(route, request, paramsDic);
        }

        public async Task<Response> Delete()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Delete<Response>(route, paramsDic);
        }
    }


    public class GenericCLApi<T> : BaseAbstractApi<T> where T : class, IEntityWithId
    {
        public GenericCLApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Create(T entity)
        {
            var paramsDic = routeParams.Copy();
            return await rest.Post<T>(route, entity, paramsDic);
        }

        public async Task<IEnumerable<T>> List()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<IEnumerable<T>>(route, paramsDic);
        }
    }
    public class GenericRUDWithIdApi<T> : BaseAbstractApi<T> where T : class, IEntityWithId
    {
        public GenericRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Read(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            return await rest.Get<T>(route + Routes.PatternId, paramsDic);
        }

        public async Task<T> Update(T entity)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, entity.Id.ToString());

            return await rest.Put<T>(route + Routes.PatternId, entity, paramsDic);
        }

        public async Task Delete(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            await rest.Delete<T>(route + Routes.PatternId, paramsDic);
        }
    }
    public class GenericRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : class
    {
        public GenericRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Read()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<T>(route, paramsDic);
        }

        public async Task<T> Update(T entity)
        {
            var paramsDic = routeParams.Copy();
            return await rest.Put<T>(route, entity, paramsDic);
        }

        public async Task Delete()
        {
            var paramsDic = routeParams.Copy();
            await rest.Delete<T>(route, paramsDic);
        }

    }
    public class GenericCRUDWithIdApi<T> : BaseAbstractApi<T> where T : class, IEntityWithId
    {
        public GenericCRUDWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Create(T entity)
        {
            var paramsDic = routeParams.Copy();
            return await rest.Post<T>(route, entity, paramsDic);
        }

        public async Task<T> Read(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            return await rest.Get<T>(route + Routes.PatternId, paramsDic);
        }

        public async Task<T> Update(T entity)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, entity.Id.ToString());

            return await rest.Put<T>(route + Routes.PatternId, entity, paramsDic);
        }

        public async Task Delete(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            await rest.Delete<T>(route + Routes.PatternId, paramsDic);
        }
    }
    public class GenericCRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : class
    {
        public GenericCRUDWithoutIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }

        public async Task<T> Create(T entity)
        {
            var paramsDic = routeParams.Copy();
            return await rest.Post<T>(route, entity, paramsDic);
        }

        public async Task<T> Read()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<T>(route, paramsDic);
        }

        public async Task<T> Update(T entity)
        {
            var paramsDic = routeParams.Copy();
            return await rest.Put<T>(route, entity, paramsDic);
        }

        public async Task Delete()
        {
            var paramsDic = routeParams.Copy();
            await rest.Delete<T>(route, paramsDic);
        }

    }
    public class GenericCRUDLWithIdApi<T> : BaseAbstractApi<T> where T : class, IEntityWithId
    {
        public GenericCRUDLWithIdApi(IRestClient rest, string route, Dictionary<string, object> routeParams) : base(rest, route, routeParams)
        {
        }


        public async Task<T> Create(T entity)
        {
            var paramsDic = routeParams.Copy();
            return await rest.Post<T>(route, entity, paramsDic);
        }

        public async Task<T> Read(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            return await rest.Get<T>(route + Routes.PatternId, paramsDic);
        }

        public async Task<T> Update(T entity)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, entity.Id.ToString());

            return await rest.Put<T>(route + Routes.PatternId, entity, paramsDic);
        }

        public async Task Delete(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());

            await rest.Delete<T>(route + Routes.PatternId, paramsDic);
        }
        public async Task<IEnumerable<T>> List()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<IEnumerable<T>>(route, paramsDic);
        }
    }


    public enum ExecuteMethod
    {
        GET,
        POST,
        PUT,
        DELETE

    }
    public class WrappedExecuteApi<T> : BaseAbstractApi<T> where T : class, IEntityWithId
    {
        private readonly ExecuteMethod method;
        public WrappedExecuteApi(IRestClient rest, string route, Dictionary<string, object> routeParams, ExecuteMethod method) : base(rest, route, routeParams)
        {
            this.method = method;
        }

        public async Task<SingleResponse<T>> ReadSingle(Guid? id = null, T data = null)
        {
            return await Read<SingleResponse<T>>(id, data);
        }

        public async Task<MultiResponse<T>> ReadMutliple(Guid? id = null, T data = null)
        {
            return await Read<MultiResponse<T>>(id, data);
        }

        private async Task<R> Read<R>(Guid? id = null, T data = null)
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
