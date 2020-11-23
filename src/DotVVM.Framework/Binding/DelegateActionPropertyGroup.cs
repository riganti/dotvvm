using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Binding
{
    public class DelegateActionPropertyGroup<TValue> : ActiveDotvvmPropertyGroup
    {
        private Action<IHtmlWriter, IDotvvmRequestContext, DotvvmPropertyGroup, DotvvmControl, IEnumerable<DotvvmProperty>> func;

        public DelegateActionPropertyGroup(PrefixArray prefixes,
            Type valueType,
            Type declaringType,
            FieldInfo descriptorField,
            string name,
            object defaultValue,
            Action<IHtmlWriter, IDotvvmRequestContext, DotvvmPropertyGroup, DotvvmControl, IEnumerable<DotvvmProperty>> func)
            : base(prefixes, valueType, declaringType, descriptorField, descriptorField, name, defaultValue)
        {
            this.func = func;
        }

        public override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context, DotvvmControl control, IEnumerable<DotvvmProperty> properties)
        {
            if (func != null) func(writer, context, this, control, properties);
        }

        public static DelegateActionPropertyGroup<TValue> Register<TDeclaringType>(
            PrefixArray prefixes,
            string name,
            Action<IHtmlWriter, IDotvvmRequestContext, DotvvmPropertyGroup, DotvvmControl, IEnumerable<DotvvmProperty>> func,
            TValue defaultValue = default(TValue))
        {
            var descriptorField = DotvvmPropertyGroup.FindDescriptorField(typeof(TDeclaringType), name);
            return (DelegateActionPropertyGroup<TValue>)DotvvmPropertyGroup.Register(
                new DelegateActionPropertyGroup<TValue>(prefixes, typeof(TValue), typeof(TDeclaringType), descriptorField, name, defaultValue, func));
        }

    }
}
