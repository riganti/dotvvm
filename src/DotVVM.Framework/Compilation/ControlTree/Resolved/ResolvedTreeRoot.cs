#nullable enable
using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using System.Linq;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedTreeRoot : ResolvedControl, IAbstractTreeRoot
    {
        public Dictionary<string, List<IAbstractDirective>> Directives { get; set; } = new Dictionary<string, List<IAbstractDirective>>(StringComparer.OrdinalIgnoreCase);
        public string? FileName { get; set; }
        public ControlBuilderDescriptor? MasterPage { get; }

        IAbstractControlBuilderDescriptor? IAbstractTreeRoot.MasterPage => MasterPage;

        public ControlBuilderDescriptor ControlBuilderDescriptor =>
            new ControlBuilderDescriptor(
                DataContextTypeStack.DataContextType,
                Metadata.Type,
                FileName,
                MasterPage,
                (from ds in Directives
                 from d in ds.Value
                 select (ds.Key, d.Value)).ToImmutableArray()
            );

        public ResolvedTreeRoot(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, ControlBuilderDescriptor? masterPage)
            : base(metadata, node, null, dataContext)
        {
            this.MasterPage = masterPage;
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
