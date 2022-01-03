using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Utils;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Compilation.ViewCompiler;

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

        public IAbstractServiceInjectDirective BuildServiceInjectDirective(
            DothtmlDirectiveNode node,
            SimpleNameBindingParserNode nameSyntax,
            BindingParserNode typeSyntax)
        {
            foreach (var syntaxNode in nameSyntax.EnumerateNodes().Concat(typeSyntax.EnumerateNodes() ?? Enumerable.Empty<BindingParserNode>()))
            {
                syntaxNode.NodeErrors.ForEach(node.AddError);
            }

            var typeDescriptor = directiveService.ResolveType(node, typeSyntax);

            if (typeDescriptor is null)
            {
                node.AddError($"{typeSyntax.ToDisplayString()} is not a valid type.");
                return new ResolvedServiceInjectDirective(nameSyntax, typeSyntax, null) { DothtmlNode = node };
            }

            return new ResolvedServiceInjectDirective(nameSyntax, typeSyntax, typeDescriptor.Type) { DothtmlNode = node };
        }

        public IAbstractImportDirective BuildImportDirective(
            DothtmlDirectiveNode node,
            BindingParserNode? aliasSyntax,
            BindingParserNode nameSyntax)
        {
            foreach (var syntaxNode in nameSyntax.EnumerateNodes().Concat(aliasSyntax?.EnumerateNodes() ?? Enumerable.Empty<BindingParserNode>()))
            {
                syntaxNode.NodeErrors.ForEach(node.AddError);
            }

            var type = directiveService.ResolveTypeOrNamespace(node, nameSyntax);

            return new ResolvedImportDirective(aliasSyntax, nameSyntax, type) { DothtmlNode = node };
        }

        public IAbstractViewModelDirective BuildViewModelDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            var type = directiveService.ResolveType(directive, nameSyntax);
            return new ResolvedViewModelDirective(nameSyntax, type!) { DothtmlNode = directive };
        }

        public IAbstractBaseTypeDirective BuildBaseTypeDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            var type = directiveService.ResolveType(directive, nameSyntax);
            return new ResolvedBaseTypeDirective(nameSyntax, type!) { DothtmlNode = directive };
        }
        public IAbstractDirective BuildViewModuleDirective(DothtmlDirectiveNode directiveNode, string modulePath, string resourceName) =>
            new ResolvedViewModuleDirective(modulePath, resourceName) { DothtmlNode = directiveNode };

        public IAbstractDirective BuildPropertyDeclarationDirective(
            DothtmlDirectiveNode directive,
            TypeReferenceBindingParserNode typeSyntax,
            SimpleNameBindingParserNode nameSyntax,
            LiteralExpressionBindingParserNode? initializer,
            IList<IAbstractDirectiveAttributeReference> resolvedAttributes,
            BindingParserNode valueSyntaxRoot)
        {
            var propertyTypeDescriptor = directiveService.ResolveType(directive, typeSyntax);

            if (propertyTypeDescriptor == null)
            {
                directive.AddError($"Could not resolve type {typeSyntax.ToDisplayString()}.");
            }

            //Chack that I am not asigning incompatible types 
            var initialValue = directiveService.ResolvePropertyInitializer(directive, propertyTypeDescriptor?.Type, initializer);

            var attributeInstances = InstantiateAttributes(resolvedAttributes).ToList();

            return new ResolvedPropertyDeclarationDirective(nameSyntax, typeSyntax, propertyTypeDescriptor, initialValue, resolvedAttributes, attributeInstances) {
                DothtmlNode = directive
            };
        }

        private IEnumerable<object> InstantiateAttributes(IList<IAbstractDirectiveAttributeReference> resolvedAttributes)
        {
            var attributePropertyGrouping = resolvedAttributes.GroupBy(
                a => a.Type.FullName,
                a => a,
                (name, attributes) => {

                    var attributeType = attributes.First().CastTo<ResolvedTypeDescriptor>().Type;
                    var properties = attributes.Select(a => (name: a.NameSyntax.Name, value: a.Initializer.Value));


                    return (attributeType, properties);
                }).ToList();

            foreach (var grouping in attributePropertyGrouping)
            {
                var attributeInstance = Activator.CreateInstance(grouping.attributeType);

                foreach (var property in grouping.properties)
                {
                    grouping.attributeType.GetProperty(property.name).SetValue(attributeInstance, property.value);
                }
                yield return attributeInstance;
            }
        }

        public IAbstractDirectiveAttributeReference BuildPropertyDeclarationAttributeReferenceDirective(
            DothtmlDirectiveNode directiveNode,
            IdentifierNameBindingParserNode propertyNameSyntax,
            ActualTypeReferenceBindingParserNode typeSyntax,
            LiteralExpressionBindingParserNode initializer)
        {
            var typeDescriptor = directiveService.ResolveType(directiveNode, typeSyntax);

            if (typeDescriptor == null)
            {
                directiveNode.AddError($"Could not resolve type {typeSyntax.ToDisplayString()} when trying to resolve property attribute type.");
            }

            return new ResolvedPropertyDirectiveAttributeReference(typeSyntax, propertyNameSyntax, typeDescriptor, initializer);
        }

        public IAbstractBaseTypeDirective Build(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            var type = directiveService.ResolveType(directive, nameSyntax);
            return new ResolvedBaseTypeDirective(nameSyntax, type) { DothtmlNode = directive };
        }

        public IAbstractDirective BuildDirective(DothtmlDirectiveNode node)
        {
            return new ResolvedDirective() { DothtmlNode = node };
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
