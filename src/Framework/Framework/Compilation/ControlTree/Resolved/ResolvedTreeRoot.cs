using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using System.Linq;
using System.Collections.Immutable;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedTreeRoot : ResolvedControl, IAbstractTreeRoot
    {
        public Dictionary<string, List<IAbstractDirective>> Directives { get; set; } = new Dictionary<string, List<IAbstractDirective>>(StringComparer.OrdinalIgnoreCase);
        public string? FileName { get; set; }
        public ControlBuilderDescriptor? MasterPage { get; }

        IAbstractControlBuilderDescriptor? IAbstractTreeRoot.MasterPage => MasterPage;

        public new DothtmlNode DothtmlNode => base.DothtmlNode.NotNull("View must have a DothtmlNode");

        public ControlBuilderDescriptor ControlBuilderDescriptor =>
            new ControlBuilderDescriptor(
                DataContextTypeStack.DataContextType,
                Metadata.Type,
                FileName,
                MasterPage,
                (from ds in Directives
                 from d in ds.Value
                 select (ds.Key, d.Value)).ToImmutableArray(),
                GetViewModuleInfo(),
                CollectContentPlaceHolderIds()
            );

        private ViewModuleReferenceInfo? GetViewModuleInfo()
        {
            if (TryGetProperty(Internal.ReferencedViewModuleInfoProperty, out var viewModule) && viewModule is ResolvedPropertyValue value)
                return value.Value as ViewModuleReferenceInfo;
            else
                return null;
        }

        /// <summary>
        /// Traverses the entire resolved tree (including controls inside templates) to collect
        /// all ContentPlaceHolder IDs declared in this page/master page file.
        /// </summary>
        private ImmutableArray<string> CollectContentPlaceHolderIds()
        {
            var collector = new ContentPlaceHolderIdCollector();
            this.AcceptChildren(collector);
            return collector.Ids.Count == 0
                ? ImmutableArray<string>.Empty
                : collector.Ids.ToImmutableArray();
        }

        private sealed class ContentPlaceHolderIdCollector : ResolvedControlTreeVisitor
        {
            public readonly List<string> Ids = new List<string>();

            public override void VisitControl(ResolvedControl control)
            {
                if (control.Metadata.Type == typeof(ContentPlaceHolder)
                    && control.Properties.TryGetValue(DotvvmControl.IDProperty, out var idSetter)
                    && idSetter is ResolvedPropertyValue { Value: string id })
                {
                    Ids.Add(id);
                }
                DefaultVisit(control);
            }
        }

        public ResolvedTreeRoot(ControlResolverMetadata metadata, DothtmlNode node, DataContextStack dataContext, ImmutableDictionary<string, ImmutableList<IAbstractDirective>> directives, ControlBuilderDescriptor? masterPage)
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
