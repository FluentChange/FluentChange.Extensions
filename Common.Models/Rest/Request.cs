namespace FluentChange.Extensions.Common.Models.Rest
{
    public class Request
    {

    }

    public class SingleRequest<T> : Request where T : new()
    {
        public T Data { get; set; }

    }
}
