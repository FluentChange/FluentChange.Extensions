namespace FluentChange.Extensions.Common.Rest
{
    public class Request
    {

    }

    public class SingleRequest<T> : Request where T : new()
    {
        public T Data { get; set; }

    }
}
