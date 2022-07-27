using System.Collections.Generic;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public sealed class ResolvedPropertyTemplate : ResolvedPropertySetter, IAbstractPropertyTemplate
    {
        public List<ResolvedControl> Content { get; set; }

        IEnumerable<IAbstractControl> IAbstractPropertyTemplate.Content => Content;

        public ResolvedPropertyTemplate(DotvvmProperty property, List<ResolvedControl> content) : base(property)
        {
            Content = content;
            content.ForEach(c => c.Parent = this);
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitPropertyTemplate(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var c in Content)
            {
                c.Accept(visitor);
            }
        }

        public override string ToString() => $"{Property}={string.Join("", Content)}";
    }
}
