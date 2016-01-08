using System;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public abstract class TreeBuilderBase
    {

        public IAbstractTreeRoot BuildRoot(IControlTreeResolver resolver, IControlResolverMetadata viewMetadata, DothtmlRootNode root)
        {
            // create the node
            var view = BuildRootCore(viewMetadata, root);

            // copy directives
            foreach (var directive in root.Directives)
            {
                if (!string.Equals(directive.Name, Constants.BaseTypeDirective, StringComparison.InvariantCultureIgnoreCase))
                {
                    view.Directives.Add(directive.Name, directive.Value);
                }
            }

            return view;
        }

        protected abstract ResolvedTreeRoot BuildRootCore(IControlResolverMetadata viewMetadata, DothtmlRootNode root);
    }
}