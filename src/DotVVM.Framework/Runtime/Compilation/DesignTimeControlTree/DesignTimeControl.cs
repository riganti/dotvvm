using System;
using System.Collections.Generic;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.DesignTimeControlTree
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

        public IEnumerable<object> ContructorParameters { get; }
    }
}