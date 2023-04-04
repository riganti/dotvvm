using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Compilation.Directives
{
    public class MasterPageDirectiveCompiler : DirectiveCompiler<IAbstractDirective, IAbstractDirective?>
    {
        public MasterPageDirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder)
            : base(directiveNodesByName, treeBuilder)
        {
        }

        public override string DirectiveName => ParserConstants.MasterPageDirective;

        protected override IAbstractDirective? CreateArtefact(IReadOnlyList<IAbstractDirective> resolvedDirectives)
        {
            return resolvedDirectives.FirstOrDefault();
        }

        protected override IAbstractDirective Resolve(DothtmlDirectiveNode d) => TreeBuilder.BuildDirective(d);
    }
}
