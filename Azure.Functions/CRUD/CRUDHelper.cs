namespace FluentChange.Extensions.Azure.Functions.CRUDL
{
    public static class CRUDHelper
    {

        public const string Id = "/{id?}";


        public static string Route(string route)
        {
            if (!route.EndsWith(Id))
            {
                route = route + "/{id?}";
            }
            return route;
        }

    }
}
