using System.Collections.Generic;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    [ControlMarkupOptions(DefaultContentProperty = nameof(Property))]
    public class ClassWithDefaultDotvvmControlContent : HtmlGenericControl
    {
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public List<DotvvmControl> Property
        {
            get { return (List<DotvvmControl>)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }
        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<List<DotvvmControl>, ClassWithDefaultDotvvmControlContent>(c => c.Property, null);
    }
}
