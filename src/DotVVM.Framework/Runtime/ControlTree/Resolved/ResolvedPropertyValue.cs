using System.Diagnostics;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    [DebuggerDisplay("{Property.Name} = \"{Value}\"")]
    public class ResolvedPropertyValue : ResolvedPropertySetter, IAbstractPropertyValue
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
