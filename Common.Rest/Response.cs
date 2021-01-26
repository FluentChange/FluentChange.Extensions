using System.Collections.Generic;

namespace FluentChange.Extensions.Common.Rest
{
    public class Response
    {
        public List<Error> Errors { get; set; }

        public Response()
        {
            Errors = new List<Error>();
        }
    }

    public class SingleResponse<T> : Response where T : class
    {
        public T Result { get; set; }

    }

    public class MultiResponse<T> : Response where T : class
    {
        public List<T> Results { get; set; }

        public MultiResponse() : base()
        {
            Results = new List<T>();
        }

    }
}
