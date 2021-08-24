using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Binding
{
    public abstract class ActiveDotvvmPropertyGroup : DotvvmPropertyGroup
    {
        protected internal ActiveDotvvmPropertyGroup(PrefixArray prefixes, Type valueType, Type declaringType, FieldInfo descriptorField, ICustomAttributeProvider attributeProvider, string name, object defaultValue) : base(prefixes, valueType, declaringType, descriptorField, attributeProvider, name, defaultValue)
        {
        }

        public abstract void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context, DotvvmControl control, IEnumerable<DotvvmProperty> properties);
    }
}
