using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Testing.SeleniumGenerator.Tests.Helpers
{
    public static class VisitorHelper
    {
        public static string TryGetNameFromProperty(ResolvedControl control, DotvvmProperty property)
        {
            if (control.TryGetProperty(property, out IAbstractPropertySetter setter))
            {
                switch (setter)
                {
                    case ResolvedPropertyValue propertySetter:
                        return propertySetter.Value?.ToString();

                    case ResolvedPropertyBinding propertyBinding:
                        return propertyBinding.Binding.Value;

                    default:
                        return "";
                }
            }

            return null;
        }
    }
}
