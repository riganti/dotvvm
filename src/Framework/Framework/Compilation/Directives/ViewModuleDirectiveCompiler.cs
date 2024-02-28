using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.ResourceManagement;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.Directives
{
    using DirectiveDictionary = ImmutableDictionary<string, ImmutableList<DothtmlDirectiveNode>>;

    public class ViewModuleDirectiveCompiler : DirectiveCompiler<IAbstractViewModuleDirective, ViewModuleCompilationResult?>
    {
        private readonly bool isMarkupControl;
        private readonly DotvvmResourceRepository resourceRepo;

        public ViewModuleDirectiveCompiler(DirectiveDictionary directiveNodesByName, IAbstractTreeBuilder treeBuilder, bool isMarkupControl, DotvvmResourceRepository resourceRepo)
            : base(directiveNodesByName, treeBuilder)
        {
            this.isMarkupControl = isMarkupControl;
            this.resourceRepo = resourceRepo;
        }

        public override string DirectiveName => ParserConstants.ViewModuleDirective;

        protected override ViewModuleCompilationResult? CreateArtefact(ImmutableList<IAbstractViewModuleDirective> resolvedDirectives)
        {
            return ResolveImportedViewModules(resolvedDirectives);
        }

        private ViewModuleCompilationResult? ResolveImportedViewModules(ImmutableList<IAbstractViewModuleDirective> moduleDirectives)
        {
            if (moduleDirectives.Count == 0)
            {
                return null;
            }

            var resources =
                moduleDirectives
                .Select(x => {
                    if (resourceRepo is object && x.DothtmlNode is object)
                    {
                        var resource = resourceRepo.FindResource(x.ImportedResourceName);
                        var node = (x.DothtmlNode as DothtmlDirectiveNode)?.ValueNode ?? x.DothtmlNode;
                        if (resource is null)
                        {
                            node.AddError($"Cannot find resource named '{x.ImportedResourceName}' referenced by the @js directive!");
                        }
                        else if (!(resource is ScriptModuleResource))
                        {
                            node.AddError($"The resource named '{x.ImportedResourceName}' referenced by the @js directive must be of the ScriptModuleResource type!");
                        }
                    }
                    return x.ImportedResourceName;
                })
                .ToArray();

            return new ViewModuleCompilationResult(
                new JsExtensionParameter(null, isMarkupControl),
                new ViewModuleReferenceInfo(null, resources, isMarkupControl));
        }

        protected override IAbstractViewModuleDirective Resolve(DothtmlDirectiveNode directiveNode) =>
            TreeBuilder.BuildViewModuleDirective(directiveNode, modulePath: directiveNode.Value, resourceName: directiveNode.Value);
    }

}
