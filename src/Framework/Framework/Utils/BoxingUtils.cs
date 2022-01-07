using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DotVVM.Framework.Utils
{
    public static class BoxingUtils
    {
        // see https://github.com/dotnet/runtime/issues/7079 for context
        // runtime can't do this optimization because it's against the spec...
        public static readonly object True = true;
        public static readonly object False = false;
        private const uint IntegersCount = 32;
        private static readonly object[] Integers = new object[] { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };
        public static readonly object Zero = Box(0);

        public static object Box(bool v) => v ? True : False;
        public static object? Box(bool? v) => v.HasValue ? Box(v.GetValueOrDefault()) : null;
        public static object Box(int v)
        {
            Debug.Assert(IntegersCount == Integers.Length);
            var index = v + 1;
            if ((uint)index < IntegersCount)
                return Integers[index];
            else
                return (object)v;
        }
        public static object? Box(int? v) => v.HasValue ? Box(v.GetValueOrDefault()) : null;

        public static object? BoxGeneric<T>(T v)
        {
            if (typeof(T).IsValueType)
            {
                // these if should be specialized by the JIT, see
                // https://sharplab.io/#v2:EYLgxg9gTgpgtADwGwBYA0AXEBDAzgWwB8ABAJgEYBYAKGIAYACY8gOgCUBXAOwwEt8YLAMIR8AB14AbGFADKMgG68wMXAG4a9BrIAW2KGIAy2YO258BG6poDMTUgwBCEBLy4BzAKp9JuGgwCGAG9/QLDiO1hsABMILkkATwYIYAArGDAMBgAVKA4YBgBeBgw8mCsw8MiYGLjE5LSMrIAxbF8C4tL8isqAsSheBWwMAsguXCyONyyASR4YdxlcEXMihhtSHt7+weGCqNj4pJT0zIBtAF0GOZHFqFw1rhgAdwbTjEvghjhyNAY6P6/BikP42P7oBgAVj+SD+AHY/gAOP4ATkBAIY5CB5BBmLBmIh5GhmNhmIRmORmLRwIxFD+pFxpHxpAhpGJpFJpHJpEppGpNkYAF8rKFehE3k0nC4ABTACAQSQMBQASiKAD4lQwAPw5MoMEAMVrtLaVcUnJo65wIWXyyQ6lXqpUsAASeAAam18tqpdaFCwAOIwDAeyT5ADyUAAIjAAGbYDiSDDS5Wqg1cBOSE1VCWZH3S6ZK5Wiyohai9XpDKAMNzRGAINYKBgAakxWfLvBjDGl0qmPGVNbrDAAPNd5ndlhBzEWy+XZ8Q4aPbkszgOEBc270YO1i7PwgvpebMsqFBuGIKd4EzY1MpaZdN7arChq/a7cCGvbffQGg++YBHo3GCZJim+oMOmkiZjQF4BGcABSvAYIGTwDGA0oYAkYgwBAMY2gqKZ/PBiEwMhyhoRhWE4dMKYXNBTB2IeGCfkhMjKEO2RqtK2SFrRpa7oEHZduhmHYZxyosDMb6ejA2TkdOfEBLx8lhAA9MpJQ6KoBQCbgOiTpI0QMMABS4JhYC8G0vAAF4wAZwBJBgGkMLBMzZLRs4CWRwk4dkj6dORIlynhclKYEikhaaC5WtKnjjNgMaCAAgrgbF/IFkgcbAnYqsqp6zueM7hVuuBaZ2nkUaJRR+V5+Z9sF4VheFl6RTKMW4HFiXJdkfzTBlsaFjlbnlvlSnDfJ85Kqeo0BIKQA
                if (typeof(T) == typeof(bool))
                {
                    return Box(Unsafe.As<T, bool>(ref v));
                }
                else if (typeof(T) == typeof(int))
                {
                    return Box(Unsafe.As<T, int>(ref v));
                }
                else if (typeof(T) == typeof(bool?))
                {
                    return Box(Unsafe.As<T, bool?>(ref v));
                }
                else if (typeof(T) == typeof(int?))
                {
                    return Box(Unsafe.As<T, int?>(ref v));
                }
            }
            return v;
        }
    }
}
