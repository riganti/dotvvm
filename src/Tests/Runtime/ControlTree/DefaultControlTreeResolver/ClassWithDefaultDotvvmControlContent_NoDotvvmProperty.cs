using System.Collections.Generic;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    [ControlMarkupOptions(DefaultContentProperty = nameof(Property))]
    public class ClassWithDefaultDotvvmControlContent_NoDotvvmProperty : HtmlGenericControl
    {
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public List<DotvvmControl> Property { get; set; }
    }
}
