#nullable enable
namespace DotVVM.Framework.Utils
{
    public static class StringUtils
    {
        public static string LimitLength(this string source, int length, string ending = "...")
        {
            if (length < source.Length)
            {
                return source.Substring(0, length - ending.Length) + ending;
            }
            else
            {
                return source;
            }
        }
    }
}
