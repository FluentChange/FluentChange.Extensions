using FluentChange.Extensions.Common.Models.Rest;
using FluentChange.Extensions.System.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FluentChange.Extensions.Common.Rest
{
   
    public class CompleteWrappedApi<TModel, TId> where TModel : class where TId : struct
    {
        protected readonly IRestClient rest;
        protected readonly string route;
        protected Dictionary<string, object> routeParams;

        static CompleteWrappedApi()
        {
            var genericType = typeof(TId);
            if (genericType != typeof(string) && genericType != typeof(Guid) && genericType != typeof(int))
            {
                throw new InvalidConstraintException("Only int, string or guid supported");
            }
        }

        public CompleteWrappedApi(IRestClient rest, string route, Dictionary<string, object> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;
        }
        public CompleteWrappedApi(IRestClient rest, string route)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = new Dictionary<string, object>();
        }


        public async Task<DataResponse<TModel>> Read() 
        {
            var paramsDic = routeParams.Copy();
            var read = rest.Get<DataResponse<TModel>>(route, paramsDic);
            return await read;
        }
        public async Task<DataResponse<TModel>> Read(TId id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            var read = rest.Get<DataResponse<TModel>>(route + Routes.PatternId, paramsDic);
            return await read;
        }


        public async Task<DataResponse<TModel>> Update(TModel entity)
        {
            var request = new SingleRequest<TModel>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Put<DataResponse<TModel>>(route, request, paramsDic);
        }
        public async Task<DataResponse<TModel>> Update(TId id, TModel entity)
        {
            var request = new SingleRequest<TModel>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Put<DataResponse<TModel>>(route + Routes.PatternId, request, paramsDic);
        }

        public async Task<DataResponse<TModel>> Create(TModel entity) 
        {
            var request = new SingleRequest<TModel>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();

            return await rest.Post<DataResponse<TModel>>(route, request, paramsDic);
        }
        public async Task<DataResponse<TModel>> Create(TId id, TModel entity)
        {
            var request = new SingleRequest<TModel>();
            request.Data = entity;
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Post<DataResponse<TModel>>(route + Routes.PatternId, request, paramsDic);
        }

        public async Task<DataResponse<IEnumerable<TModel>>> List() 
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<DataResponse<IEnumerable<TModel>>>(route, paramsDic);
        }
        public async Task<DataResponse<IEnumerable<TModel>>> List(TId id) 
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Get<DataResponse<IEnumerable<TModel>>>(route + Routes.PatternId, paramsDic);
        }

        public async Task<Response> Delete()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Delete<Response>(route, paramsDic);
        }
        public async Task<Response> Delete(TId id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Delete<Response>(route + Routes.PatternId, paramsDic);
        }


    }
    public class CompleteApi<TModel, TId> where TModel : class where TId : struct
    {
        protected readonly IRestClient rest;
        protected readonly string route;
        protected Dictionary<string, object> routeParams;

        static CompleteApi()
        {
            var genericType = typeof(TId);
            if (genericType != typeof(string) && genericType != typeof(Guid) && genericType != typeof(int))
            {
                throw new InvalidConstraintException("Only int, string or guid supported");
            }
        }

        public CompleteApi(IRestClient rest, string route, Dictionary<string, object> routeParams)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = routeParams;
        }
        public CompleteApi(IRestClient rest, string route)
        {
            this.rest = rest;
            this.route = route;
            this.routeParams = new Dictionary<string, object>();
        }


        public async Task<TModel> Read()
        {
            var paramsDic = routeParams.Copy();
            var read = rest.Get<TModel>(route, paramsDic);
            return await read;
        }
        public async Task<TModel> Read(TId id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            var read = rest.Get<TModel>(route + Routes.PatternId, paramsDic);
            return await read;
        }


        public async Task<TModel> Update(TModel entity)
        {
            var paramsDic = routeParams.Copy();
            return await rest.Put<TModel>(route, entity, paramsDic);
        }
        public async Task<TModel> Update(TId id, TModel entity)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Put<TModel>(route + Routes.PatternId, entity, paramsDic);
        }

        public async Task<TModel> Create(TModel entity)
        {  
            var paramsDic = routeParams.Copy();
            return await rest.Post<TModel>(route, entity, paramsDic);
        }
        public async Task<TModel> Create(TId id, TModel entity)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Post<TModel>(route + Routes.PatternId, entity, paramsDic);
        }

        public async Task<IEnumerable<TModel>> List()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Get<IEnumerable<TModel>>(route, paramsDic);
        }
        public async Task<IEnumerable<TModel>> List(TId id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Get<IEnumerable<TModel>>(route + Routes.PatternId, paramsDic);
        }

        public async Task<Response> Delete()
        {
            var paramsDic = routeParams.Copy();
            return await rest.Delete<Response>(route, paramsDic);
        }
        public async Task<Response> Delete(TId id)
        {
            var paramsDic = routeParams.Copy();
            paramsDic.Add(Routes.ParamNameId, id.ToString());
            return await rest.Delete<Response>(route + Routes.PatternId, paramsDic);
        }


    }
}
