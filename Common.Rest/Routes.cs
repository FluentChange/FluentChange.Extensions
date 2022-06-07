using System.Text;

namespace FluentChange.Extensions.Common.Rest
{
    public static class Routes
    {
        public const string ParamNameId = "id";
        public const string PatternId = "/{" + ParamNameId + "}";
        public const string OptionalId = "/{" + ParamNameId + "?}";

        public const string dummy = "/{" + ParamNameId + "?}";
    }
}
