#nullable enable
using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using System.Linq;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedTreeRoot : ResolvedContentNode, IAbstractTreeRoot
    {
        public Dictionary<string, List<IAbstractDirective>> Directives { get; set; } = new Dictionary<string, List<IAbstractDirective>>(StringComparer.OrdinalIgnoreCase);
        public string? FileName { get; set; }

        public ResolvedTreeRoot(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives)
            : base(metadata, node, null, dataContext)
        {
            Directives = directives.ToDictionary(d => d.Key, d => d.Value.ToList());
            foreach (var ds in Directives.Values) foreach (var d in ds)
                    ((ResolvedDirective)d).Parent = this;
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var dir in Directives.Values)
            {
                dir.ForEach(d => (d as ResolvedDirective)?.Accept(visitor));
            }

            base.AcceptChildren(visitor);
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitView(this);
        }
    }
}
