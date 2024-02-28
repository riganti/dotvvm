using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation.Directives
{
    using DirectiveDictionary = ImmutableDictionary<string, ImmutableList<DothtmlDirectiveNode>>;

    public class DefaultDirectiveResolver : DirectiveResolver<IAbstractDirective>
    {
        private readonly IAbstractTreeBuilder treeBuilder;

        public DefaultDirectiveResolver(DirectiveDictionary directiveNodesByName, IAbstractTreeBuilder treeBuilder)
            : base(directiveNodesByName)
        {
            this.treeBuilder = treeBuilder;
        }

        public ImmutableList<IAbstractDirective> ResolveAll(string directiveName) => Resolve(directiveName);
        protected override IAbstractDirective Resolve(DothtmlDirectiveNode d) => treeBuilder.BuildDirective(d);
    }

}
