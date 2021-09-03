using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedPropertyValue : ResolvedPropertySetter, IAbstractPropertyValue
    {
        public object? Value { get; set; }

        public ResolvedPropertyValue(DotvvmProperty property, object? value) : base(property)
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

        private static string DebugFormatValue(object? v) =>
            v is null ? "null" :
            v is IEnumerable<object> vs ? $"[{string.Join(", ", vs.Select(DebugFormatValue))}]" :
            v.ToString();

        public override string ToString() => $"{Property}=\"{DebugFormatValue(Value)}\"";
    }
}
