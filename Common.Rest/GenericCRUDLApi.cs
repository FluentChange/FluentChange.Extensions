using FluentChange.Extensions.System.Helper;
using System;
using System.Collections.Generic;


namespace FluentChange.Extensions.Common.Rest
{
    public class GenericCRUDLApi<T> where T : class, IEntityWithId
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
              

        public SingleResponse<T> Create(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return rest.Post<SingleResponse<T>>(route, request, paramsDic);
        }

        public SingleResponse<T> Read(Guid id)
        {
            var paramsDic = routeParams.Copy();       
            paramsDic.Add(Routes.ParamNameId, id.ToString());      

            return rest.Get<SingleResponse<T>>(route + Routes.OptionalId, paramsDic);
        }

       

        public SingleResponse<T> Update(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, entity.Id.ToString());
 
            return rest.Put<SingleResponse<T>>(route + Routes.OptionalId, request, paramsDic);
        }

        public void Delete(Guid id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
   
            rest.Delete<SingleResponse<T>>(route + Routes.OptionalId, paramsDic);
        }
        public MultiResponse<T> List()
        {
            var paramsDic = routeParams.Copy();
            return rest.Get<MultiResponse<T>>(route, paramsDic);
        }

      
    }
}
