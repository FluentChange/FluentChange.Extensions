using System;
using System.Collections.Generic;

namespace FluentChange.Extensions.Common.Models.Rest
{
    public class Response
    {
        public List<ErrorInfo> Errors { get; set; }

        public Response()
        {
            Errors = new List<ErrorInfo>();
        }
    }

    public class DataResponse<T> : Response
    {
        public T Data { get; set; }

    }


    [Obsolete("Please use DataResponse")]
    public class NewResponse<T> : Response 
    {
        public T Data { get; set; }

    }
    [Obsolete("Please use NewResponse")]
    public class SingleResponse<T> : Response where T : class
    {
        public T Result { get; set; }

    }

    [Obsolete("Please use NewResponse")]
    public class MultiResponse<T> : Response where T : class
    {
        public List<T> Results { get; set; }

        public MultiResponse() : base()
        {
            Results = new List<T>();
        }

    }

   
}
