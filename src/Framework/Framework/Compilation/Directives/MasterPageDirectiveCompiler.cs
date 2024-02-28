using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.Directives
{
    using DirectiveDictionary = ImmutableDictionary<string, ImmutableList<DothtmlDirectiveNode>>;

    public class MasterPageDirectiveCompiler : DirectiveCompiler<IAbstractDirective, IAbstractDirective?>
    {
        public MasterPageDirectiveCompiler(DirectiveDictionary directiveNodesByName, IAbstractTreeBuilder treeBuilder)
            : base(directiveNodesByName, treeBuilder)
        {
        }

        public override string DirectiveName => ParserConstants.MasterPageDirective;

        protected override IAbstractDirective? CreateArtefact(ImmutableList<IAbstractDirective> resolvedDirectives)
        {
            return resolvedDirectives.FirstOrDefault();
        }

        protected override IAbstractDirective Resolve(DothtmlDirectiveNode d) => TreeBuilder.BuildDirective(d);
    }
}
