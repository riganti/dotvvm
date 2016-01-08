using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree.DesignTime
{
    public class DesignTimeControl : DesignTimeContentNode, IAbstractControl
    {
        public DesignTimeControl(DothtmlNode node, DesignTimeControlResolver resolver) : base(node, resolver)
        {
        }

        public IEnumerable<IPropertyDescriptor> PropertyNames { get; }

        public bool TryGetProperty(IPropertyDescriptor property, out IAbstractPropertySetter value)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyDictionary<string, object> HtmlAttributes { get; }

        public object[] ConstructorParameters { get; set; }
    }
}