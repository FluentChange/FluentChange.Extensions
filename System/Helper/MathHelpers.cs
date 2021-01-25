using System;

namespace FluentChange.Extensions.System.Helper
{
    public static class MathHelpers
    {

        public static double ToDegrees(this double r)
        {
            return r * 180 / Math.PI;
        }

        public static double ToRadian(this double d)
        {
            return d * (Math.PI / 180);
        }
    }
}
