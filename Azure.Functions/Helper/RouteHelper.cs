namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public static class RouteHelper
    {
        public const string Id = "/{id?}";

        public static string ExtendWithIdIfNeeded(string route)
        {
            if (!route.EndsWith(Id))
            {
                route = route + "/{id?}";
            }
            return route;
        }
    }
}
