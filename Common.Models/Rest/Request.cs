namespace FluentChange.Extensions.Common.Models.Rest
{
    public class Request
    {

    }

    public class SingleRequest<T> : Request 
    {
        public T Data { get; set; }

    }
}
