using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI
{
    public static class ControlHelpers
    {

        public static void CopyProperty(DotvvmBindableObject source, DotvvmProperty sourceProperty, DotvvmBindableObject target, DotvvmProperty targetProperty)
        {
            var binding = source.GetValueBinding(sourceProperty);
            if (binding != null)
            {
                target.SetBinding(targetProperty, binding);
            }
            else
            {
                target.SetValue(targetProperty, source.GetValue(sourceProperty));
            }
        }

        public static string ConcatCssClasses(params string?[] fragments)
        {
            return string.Join(" ", fragments.Select(f => f?.Trim() ?? "").Where(f => !string.IsNullOrEmpty(f)));
        }
    }
}
