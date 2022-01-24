using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeBuilder
    {
        IAbstractTreeRoot BuildTreeRoot(IControlTreeResolver controlTreeResolver, IControlResolverMetadata metadata, DothtmlRootNode node, IDataContextStack dataContext, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives, IAbstractControlBuilderDescriptor? masterPage);

        IAbstractControl BuildControl(IControlResolverMetadata metadata, DothtmlNode? node, IDataContextStack dataContext);

        IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, IDataContextStack dataContext, DothtmlBindingNode node, IPropertyDescriptor property);

        IAbstractDirective BuildDirective(DothtmlDirectiveNode node);

        IAbstractImportDirective BuildImportDirective(DothtmlDirectiveNode node, BindingParserNode? aliasSyntax, BindingParserNode nameSyntax);
        IAbstractServiceInjectDirective BuildServiceInjectDirective(DothtmlDirectiveNode node, SimpleNameBindingParserNode nameSyntax, BindingParserNode typeSyntax, ImmutableList<NamespaceImport> imports);

        IAbstractViewModelDirective BuildViewModelDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax, ImmutableList<NamespaceImport> imports);

        IAbstractBaseTypeDirective BuildBaseTypeDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax, ImmutableList<NamespaceImport> imports);

        IAbstractViewModuleDirective BuildViewModuleDirective(DothtmlDirectiveNode directiveNode, string modulePath, string resourceName);
        IAbstractPropertyDeclarationDirective BuildPropertyDeclarationDirective(DothtmlDirectiveNode directive, TypeReferenceBindingParserNode typeSyntax, SimpleNameBindingParserNode nameSyntax, BindingParserNode? initializer, IList<IAbstractDirectiveAttributeReference> resolvedAttributes, BindingParserNode valueSyntaxRoot, ImmutableList<NamespaceImport> imports);
        IAbstractDirectiveAttributeReference BuildPropertyDeclarationAttributeReference(DothtmlDirectiveNode directiveNode, IdentifierNameBindingParserNode propertyNameSyntax, ActualTypeReferenceBindingParserNode typeSyntax, LiteralExpressionBindingParserNode initializer, ImmutableList<NamespaceImport> imports);
        IAbstractPropertyBinding BuildPropertyBinding(IPropertyDescriptor property, IAbstractBinding binding, DothtmlAttributeNode? sourceAttributeNode);

        IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl? control, DothtmlElementNode? wrapperElementNode);

        IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls, DothtmlElementNode? wrapperElementNode);

        IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls, DothtmlElementNode? wrapperElementNode);

        IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object? value, DothtmlNode? sourceAttributeNode);

        bool AddProperty(IAbstractControl control, IAbstractPropertySetter setter, [NotNullWhen(false)] out string? error);

        void AddChildControl(IAbstractContentNode control, IAbstractControl child);
    }
}
