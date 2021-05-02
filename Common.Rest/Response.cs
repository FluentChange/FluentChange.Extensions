using System.Collections.Generic;

namespace FluentChange.Extensions.Common.Rest
{
    public class Response
    {
        public List<ErrorInfo> Errors { get; set; }

        public Response()
        {
            Errors = new List<ErrorInfo>();
        }
    }

    public class SingleResponse<T> : Response where T : new()
    {
        public T Result { get; set; }

    }

    public class MultiResponse<T> : Response where T : new()
    {
        public List<T> Results { get; set; }

        public MultiResponse() : base()
        {
            Results = new List<T>();
        }

    }
}
