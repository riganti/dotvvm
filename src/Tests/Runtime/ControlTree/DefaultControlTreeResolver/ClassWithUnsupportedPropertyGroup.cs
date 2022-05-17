using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using System.Collections.Generic;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    public class ClassWithUnsupportedPropertyGroup : HtmlGenericControl
    {
        [PropertyGroup("MyGroup:")]
        public Dictionary<string, bool> MyGroup { get; set; }
    }
}
