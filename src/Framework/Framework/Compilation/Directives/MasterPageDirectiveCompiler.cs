using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;
using System.Linq;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.Compilation.Directives
{
    public class MasterPageDirectiveCompiler : DirectiveCompiler<IAbstractDirective, IAbstractControlBuilderDescriptor?>
    {
        private readonly IControlBuilderFactory controlBuilderFactory;
        private readonly ITypeDescriptor? viewModel;

        public MasterPageDirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder, IControlBuilderFactory controlBuilderFactory, ITypeDescriptor? viewModel)
            : base(directiveNodesByName, treeBuilder)
        {
            this.controlBuilderFactory = controlBuilderFactory;
            this.viewModel = viewModel;
        }

        public override string DirectiveName => ParserConstants.MasterPageDirective;

        protected override IAbstractControlBuilderDescriptor? CreateArtefact(IReadOnlyList<IAbstractDirective> resolvedDirectives)
        {
            var masterPageDirective = resolvedDirectives.FirstOrDefault();

            if(masterPageDirective == null)
            {
                return null;
            }

            try
            {
                var masterPage = controlBuilderFactory.GetControlBuilder(masterPageDirective.Value).descriptor;
                ValidateMasterPage(viewModel, masterPageDirective, masterPage);
                return masterPage;
            }
            catch (Exception e)
            {
                // The resolver should not just crash on an invalid directive
                masterPageDirective.DothtmlNode!.AddError(e.Message);
                return null;
            }
        }

        protected override IAbstractDirective Resolve(DothtmlDirectiveNode d) => TreeBuilder.BuildDirective(d);

        protected virtual void ValidateMasterPage(ITypeDescriptor? viewModel, IAbstractDirective masterPageDirective, IAbstractControlBuilderDescriptor? masterPage)
        {
            if (masterPage is null || viewModel is null)
            {
                return;
            }

            if (masterPage.DataContextType is ResolvedTypeDescriptor typeDescriptor && typeDescriptor.Type == typeof(UnknownTypeSentinel))
            {
                masterPageDirective!.DothtmlNode!.AddError("Could not resolve the type of viewmodel for the specified master page. " +
                    $"This usually means that there is an error with the @viewModel directive in the master page file: \"{masterPage.FileName}\". " +
                    $"Make sure that the provided viewModel type is correct and visible for DotVVM.");
            }
            else if (!masterPage.DataContextType.IsAssignableFrom(viewModel))
            {
                masterPageDirective!.DothtmlNode!.AddError($"The viewmodel {viewModel.Name} is not assignable to the viewmodel of the master page {masterPage.DataContextType.Name}.");
            }
        }
    }

}
