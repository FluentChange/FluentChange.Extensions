using FluentChange.Extensions.Common.Models;
using FluentChange.Extensions.System.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
    public class WrappedGenericCRUDApi<T> where T : new()
    {
        private readonly IRestClient rest;
        private readonly string route;
        private Dictionary<string, string> routeParams;

        public WrappedGenericCRUDApi(IRestClient rest, string route, Dictionary<string, string> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;
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
    public class WrappedGenericCRUDLApi<T> where T :  IEntityWithId, new()
    {
        private readonly IRestClient rest;
        private readonly string route;
        private Dictionary<string, string> routeParams;

        public WrappedGenericCRUDLApi(IRestClient rest, string route, Dictionary<string, string> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;   
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

    public class GenericCRUDApi<T> where T : new()
    {
        private readonly IRestClient rest;
        private readonly string route;
        private Dictionary<string, string> routeParams;

        public GenericCRUDApi(IRestClient rest, string route, Dictionary<string, string> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;
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
    public class GenericCRUDLApi<T> where T : IEntityWithId, new()
    {
        private readonly IRestClient rest;
        private readonly string route;
        private Dictionary<string, string> routeParams;
        
        public GenericCRUDLApi(IRestClient rest, string route, Dictionary<string, string> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;    
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
