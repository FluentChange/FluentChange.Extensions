using System;
using System.Collections.Generic;


namespace FluentChange.Extensions.Common.Rest
{
    public abstract class GenericCRUDLApi<T> where T : class, IEntityWithId
    {
        private readonly IRestClient rest;
        private readonly string route;
        private Action beforeEachRequest;
        public GenericCRUDLApi(IRestClient rest, string route)
        {
            this.rest = rest;
            this.route = route;
        }

        public void BeforeEachRequest(Action action)
        {
            this.beforeEachRequest = action;
        }     

        public SingleResponse<T> Create(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            beforeEachRequest.Invoke();
            return rest.Post<SingleResponse<T>>(route, request);
        }

        public SingleResponse<T> Read(Guid id)
        {
            var paramsDic = new Dictionary<string, string>();
            paramsDic.Add("id", id.ToString());

            beforeEachRequest.Invoke();

            return rest.Get<SingleResponse<T>>(route + "/{id?}", paramsDic);
        }

        public SingleResponse<T> Update(T entity)
        {
            var request = new SingleRequest<T>();
            request.Data = entity;
            var paramsDic = new Dictionary<string, string>();
            paramsDic.Add("id", entity.Id.ToString());
            beforeEachRequest.Invoke();
            return rest.Put<SingleResponse<T>>(route + "/{id?}", request, paramsDic);
        }

        public void Delete(Guid id)
        {
            var paramsDic = new Dictionary<string, string>();
            paramsDic.Add("id", id.ToString());
            beforeEachRequest.Invoke();
            rest.Delete<SingleResponse<T>>(route + "/{id?}", paramsDic);
        }
        public MultiResponse<T> List()
        {
            beforeEachRequest.Invoke();
            return rest.Get<MultiResponse<T>>(route);
        }
    }
}
