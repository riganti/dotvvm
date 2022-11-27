using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Utils;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Compilation.ViewCompiler;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedTreeBuilder : IAbstractTreeBuilder
    {
        private readonly BindingCompilationService bindingService;
        private readonly DirectiveCompilationService directiveService;

        public ResolvedTreeBuilder(BindingCompilationService bindingService, DirectiveCompilationService directiveService)
        {
            this.bindingService = bindingService;
            this.directiveService = directiveService;
        }

        public IAbstractTreeRoot BuildTreeRoot(IControlTreeResolver controlTreeResolver, IControlResolverMetadata metadata, DothtmlRootNode node, IDataContextStack dataContext, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, IAbstractControlBuilderDescriptor? masterPage)
        {
            return new ResolvedTreeRoot((ControlResolverMetadata)metadata, node, (DataContextStack)dataContext, directives, (ControlBuilderDescriptor?)masterPage);
        }

        public IAbstractControl BuildControl(IControlResolverMetadata metadata, DothtmlNode? node, IDataContextStack dataContext)
        {
            return new ResolvedControl((ControlResolverMetadata)metadata, node, (DataContextStack)dataContext);
        }

        public IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, IDataContextStack dataContext, DothtmlBindingNode node, IPropertyDescriptor? property)
        {
            return new ResolvedBinding(bindingService, bindingOptions, (DataContextStack)dataContext, node.Value, property: property as DotvvmProperty) {
                DothtmlNode = node,
            };
        }

        public IAbstractPropertyBinding BuildPropertyBinding(IPropertyDescriptor property, IAbstractBinding binding, DothtmlAttributeNode? sourceAttribute)
        {
            return new ResolvedPropertyBinding((DotvvmProperty)property, (ResolvedBinding)binding) { DothtmlNode = sourceAttribute };
        }

        public IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl? control, DothtmlElementNode? wrapperElement)
        {
            return new ResolvedPropertyControl((DotvvmProperty)property, (ResolvedControl?)control) { DothtmlNode = wrapperElement };
        }

        public IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls, DothtmlElementNode? wrapperElement)
        {
            return new ResolvedPropertyControlCollection((DotvvmProperty)property, controls.Cast<ResolvedControl>().ToList()) { DothtmlNode = wrapperElement };
        }

        public IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls, DothtmlElementNode? wrapperElement)
        {
            return new ResolvedPropertyTemplate((DotvvmProperty)property, templateControls.Cast<ResolvedControl>().ToList()) { DothtmlNode = wrapperElement };
        }

        public IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object? value, DothtmlNode? sourceNode)
        {
            return new ResolvedPropertyValue((DotvvmProperty)property, value) { DothtmlNode = sourceNode };
        }

        //TODO: Copy errors from nameSyntax
        public IAbstractServiceInjectDirective BuildServiceInjectDirective(
            DothtmlDirectiveNode node,
            SimpleNameBindingParserNode nameSyntax,
            BindingParserNode typeSyntax,
            ImmutableList<NamespaceImport> imports)
        {
            return new ResolvedServiceInjectDirective(directiveService, node, nameSyntax, typeSyntax, imports);
        }

        //TODO: Copy errors from aliasSyntax and nameSyntax
        public IAbstractImportDirective BuildImportDirective(
            DothtmlDirectiveNode node,
            BindingParserNode? aliasSyntax,
            BindingParserNode nameSyntax)
        { 
            return new ResolvedImportDirective(directiveService, node, aliasSyntax, nameSyntax);
        }

        //TODO: Copy errors from nameSyntax
        public IAbstractViewModelDirective BuildViewModelDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax, ImmutableList<NamespaceImport> imports)
        {
            return new ResolvedViewModelDirective(directiveService, directive, nameSyntax, imports);
        }

        //TODO: Copy errors from nameSyntax
        public IAbstractBaseTypeDirective BuildBaseTypeDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax, ImmutableList<NamespaceImport> imports)
        {
            return new ResolvedBaseTypeDirective(directiveService, directive, nameSyntax, imports);
        }
        public IAbstractViewModuleDirective BuildViewModuleDirective(DothtmlDirectiveNode directiveNode, string modulePath, string resourceName) =>
            new ResolvedViewModuleDirective(directiveNode, modulePath, resourceName);

        public IAbstractCsharpViewModuleDirective BuildCsharpViewModuleDirective(DothtmlDirectiveNode directiveNode, BindingParserNode typeName, ImmutableList<NamespaceImport> imports) =>
            new ResolvedCsharpViewModuleDirective(directiveService, directiveNode, typeName, imports);

        public IAbstractPropertyDeclarationDirective BuildPropertyDeclarationDirective(
            DothtmlDirectiveNode directive,
            TypeReferenceBindingParserNode typeSyntax,
            SimpleNameBindingParserNode nameSyntax,
            BindingParserNode? initializer,
            IList<IAbstractDirectiveAttributeReference> resolvedAttributes,
            BindingParserNode valueSyntaxRoot,
            ImmutableList<NamespaceImport> imports)
        {

            return new ResolvedPropertyDeclarationDirective(directiveService, directive, nameSyntax, typeSyntax, initializer, resolvedAttributes, imports);
        }

        public IAbstractDirectiveAttributeReference BuildPropertyDeclarationAttributeReference(
            DothtmlDirectiveNode directiveNode,
            IdentifierNameBindingParserNode propertyNameSyntax,
            ActualTypeReferenceBindingParserNode typeSyntax,
            LiteralExpressionBindingParserNode initializer,
            ImmutableList<NamespaceImport> imports)
        {
            return new ResolvedPropertyDirectiveAttributeReference(directiveService, directiveNode, typeSyntax, propertyNameSyntax, initializer, imports);
        }

        public IAbstractDirective BuildDirective(DothtmlDirectiveNode node)
        {
            return new ResolvedDirective(node);
        }

        public bool AddProperty(IAbstractControl control, IAbstractPropertySetter setter, [NotNullWhen(false)] out string? error)
        {
            return ((ResolvedControl)control).SetProperty((ResolvedPropertySetter)setter, false, out error);
        }

        public void AddChildControl(IAbstractContentNode control, IAbstractControl child)
        {
            ((ResolvedContentNode)control).AddChild((ResolvedControl)child);
        }

    }
}
