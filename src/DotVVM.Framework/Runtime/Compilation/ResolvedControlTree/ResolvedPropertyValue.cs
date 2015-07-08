using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    [DebuggerDisplay("{Property.Name} = \"{Value}\"")]
    public class ResolvedPropertyValue : ResolvedPropertySetter
    {
        public object Value { get; set; }

        public ResolvedPropertyValue(DotvvmProperty property, object value)
            : base(property)
        {
            this.Value = value;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitPropertyValue(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
        }
    }
}
