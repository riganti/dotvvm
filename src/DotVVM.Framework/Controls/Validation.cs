using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class Validation
    {
        [AttachedProperty(typeof(bool))]
        [MarkupOptions(AllowBinding = false)]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, Validation>(() => EnabledProperty, true, true);

        [AttachedProperty(typeof(object))]
        [MarkupOptions(AllowHardCodedValue = false)]
        public static DotvvmProperty TargetProperty = DotvvmProperty.Register<object, Validation>(() => TargetProperty, null, true);
    }
}
