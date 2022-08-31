using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI
{
    public static class ControlHelpers
    {
        public static string ConcatCssClasses(params string?[] fragments)
        {
            return string.Join(" ", fragments.Select(f => f?.Trim() ?? "").Where(f => !string.IsNullOrEmpty(f)));
        }
    }
}
