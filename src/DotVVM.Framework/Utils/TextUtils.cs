namespace DotVVM.Framework.Utils
{
    public static class TextUtils
    {
        private static readonly string[] sizeUnits = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string FormatSize(double bytes)
        {
            var order = 0;
            var size = bytes;

            while (size >= 1024 && order < sizeUnits.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return order <= 2
                ? $"{size:0.##} {sizeUnits[order]}"
                : $"{size:0.#} {sizeUnits[order]}";
        }
    }
}