using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.Controls
{
    [ContainsDotvvmProperties]
    public class AttachedProperties
    {

        [AttachedProperty(typeof(object))]
        [PropertyGroup("Bind-")]
        public static DotvvmPropertyGroup BindGroupDescriptor =
            DelegateActionPropertyGroup<object>.Register<AttachedProperties>("Bind-", "Bind", AddBindToWriter);

        private static void AddBindToWriter(IHtmlWriter writer, IDotvvmRequestContext context, DotvvmPropertyGroup group, DotvvmControl control, IEnumerable<DotvvmProperty> properties)
        {
            var bindingGroup = new KnockoutBindingGroup();
            foreach (var prop in properties)
            {
                bindingGroup.Add(prop.Name, control, prop);
            }
            writer.AddKnockoutDataBind("attr", bindingGroup);
        }
    }
}
