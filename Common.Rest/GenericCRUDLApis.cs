using FluentChange.Extensions.Common.Models;
using FluentChange.Extensions.System.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
    public abstract class BaseAbstractApi<T> where T : new()
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

    public class WrappedGenericCLApi<T> : BaseAbstractApi<T> where T : IEntityWithId, new()
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
    public class WrappedGenericCRUDWithIdApi<T> : BaseAbstractApi<T> where T : new()
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
    public class WrappedGenericCRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : new()
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
    public class WrappedGenericCRUDLWithIdApi<T> : BaseAbstractApi<T> where T : IEntityWithId, new()
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
    public class WrappedGenericRUDWithIdApi<T> : BaseAbstractApi<T> where T : new()
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
    public class WrappedGenericRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : new()
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


    public class GenericCLApi<T> : BaseAbstractApi<T> where T : IEntityWithId, new()
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
    public class GenericRUDWithIdApi<T> : BaseAbstractApi<T> where T : IEntityWithId, new()
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
    public class GenericRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : new()
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
    public class GenericCRUDWithIdApi<T> : BaseAbstractApi<T> where T : IEntityWithId, new()
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
    public class GenericCRUDWithoutIdApi<T> : BaseAbstractApi<T> where T : new()
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
    public class GenericCRUDLWithIdApi<T> : BaseAbstractApi<T> where T : IEntityWithId, new()
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
}
