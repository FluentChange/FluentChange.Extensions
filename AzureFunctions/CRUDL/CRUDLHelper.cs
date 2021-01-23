namespace FluentChange.AzureFunctions.CRUDL
{
    public static class CRUDLHelper
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
