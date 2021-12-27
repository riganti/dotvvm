using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Compilation.ViewCompiler;
using System.Reflection;
using System.Reflection.Emit;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedTreeBuilder : IAbstractTreeBuilder
    {
        private readonly BindingCompilationService bindingService;
        private readonly CompiledAssemblyCache compiledAssemblyCache;
        private readonly ExtensionMethodsCache extensionMethodsCache;

        public ResolvedTreeBuilder(BindingCompilationService bindingService, CompiledAssemblyCache compiledAssemblyCache, ExtensionMethodsCache extensionsMethodsCache)
        {
            this.bindingService = bindingService;
            this.compiledAssemblyCache = compiledAssemblyCache;
            this.extensionMethodsCache = extensionsMethodsCache;
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

            var expression = ParseDirectiveExpression(node, typeSyntax);

            if (expression is UnknownStaticClassIdentifierExpression)
            {
                node.AddError($"{typeSyntax.ToDisplayString()} is not a valid type.");
                return new ResolvedServiceInjectDirective(nameSyntax, typeSyntax, null) { DothtmlNode = node };
            }
            else if (expression is StaticClassIdentifierExpression)
            {
                return new ResolvedServiceInjectDirective(nameSyntax, typeSyntax, expression.Type) { DothtmlNode = node };
            }
            else throw new NotSupportedException();
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

            var expression = ParseDirectiveExpression(node, nameSyntax);

            if (expression is UnknownStaticClassIdentifierExpression)
            {
                var namespaceValid = expression
                    .CastTo<UnknownStaticClassIdentifierExpression>().Name
                    .Apply(compiledAssemblyCache.IsAssemblyNamespace);

                if (!namespaceValid)
                {
                    node.AddError($"{nameSyntax.ToDisplayString()} is unknown type or namespace.");
                }

                return new ResolvedImportDirective(aliasSyntax, nameSyntax, null) { DothtmlNode = node };

            }
            else if (expression is StaticClassIdentifierExpression)
            {
                return new ResolvedImportDirective(aliasSyntax, nameSyntax, expression.Type) { DothtmlNode = node };
            }

            node.AddError($"{nameSyntax.ToDisplayString()} is not a type or namespace.");
            return new ResolvedImportDirective(aliasSyntax, nameSyntax, null) { DothtmlNode = node };
        }

        public IAbstractViewModelDirective BuildViewModelDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            var type = ResolveTypeNameDirective(directive, nameSyntax);
            return new ResolvedViewModelDirective(nameSyntax, type!) { DothtmlNode = directive };
        }

        public IAbstractBaseTypeDirective BuildBaseTypeDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            var type = ResolveTypeNameDirective(directive, nameSyntax);
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
            var propertyTypeDescriptor = ResolveTypeNameDirective(directive, typeSyntax);

            if (propertyTypeDescriptor == null)
            {
                directive.AddError($"Could not resolve type {typeSyntax.ToDisplayString()}.");
            }

            //Chack that I am not asigning incompatible types 
            var initialValue = CreateInitializerValue(directive, propertyTypeDescriptor?.Type, initializer) ?? CreateDefaultValue(propertyTypeDescriptor);

            var attributeInstances = InstantiateAttributes(resolvedAttributes).ToList();

            return new ResolvedPropertyDeclarationDirective(nameSyntax, typeSyntax, propertyTypeDescriptor, initialValue, resolvedAttributes, attributeInstances) {
                DothtmlNode = directive
            };
        }

        private object? CreateInitializerValue(DothtmlDirectiveNode directive, Type? propertyType, LiteralExpressionBindingParserNode? initializer)
        {
            if (initializer == null || propertyType == null) { return null; }
            var originalLiteralType = initializer.Value.GetType();

            if (originalLiteralType != typeof(string)) { return initializer.Value; }

            var initializerValueString = initializer.Value.ToString();

            if (propertyType == typeof(char))
            {
                if (initializerValueString.Length != 1)
                {
                    directive.AddError($"Could not convert \"{initializerValueString}\" to char when initializing property {directive.Name}.");
                    return default(char);
                }

                return initializerValueString.Single();
            }

            if (propertyType == typeof(Guid))
            {
                if (Guid.TryParse(initializerValueString, out var guid))
                {
                    return guid;
                }
                directive.AddError($"Could not convert \"{initializerValueString}\" to Guid when initializing property {directive.Name}.");
                return default(Guid);
            }

            return initializerValueString;
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
            var typeDescriptor = ResolveTypeNameDirective(directiveNode, typeSyntax);

            if (typeDescriptor == null)
            {
                directiveNode.AddError($"Could not resolve type {typeSyntax.ToDisplayString()} when trying to resolve property attribute type.");
            }

            return new ResolvedPropertyDirectiveAttributeReference(typeSyntax, propertyNameSyntax, typeDescriptor, initializer);
        }



        private object? CreateDefaultValue(ResolvedTypeDescriptor descriptor)
        {
            if (descriptor != null && descriptor.Type.IsValueType)
            {
                return Activator.CreateInstance(descriptor.Type);
            }
            return null;
        }

        public IAbstractBaseTypeDirective Build(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            var type = ResolveTypeNameDirective(directive, nameSyntax);
            return new ResolvedBaseTypeDirective(nameSyntax, type) { DothtmlNode = directive };
        }

        private ResolvedTypeDescriptor? ResolveTypeNameDirective(DothtmlDirectiveNode directive, BindingParserNode nameSyntax)
        {
            if (ParseDirectiveExpression(directive, nameSyntax) is not StaticClassIdentifierExpression expression)
            {
                directive.AddError($"Could not resolve type '{nameSyntax.ToDisplayString()}'.");
                return null;
            }
            else return new ResolvedTypeDescriptor(expression.Type);
        }


        private Expression? ParseDirectiveExpression(DothtmlDirectiveNode directive, BindingParserNode expressionSyntax)
        {
            TypeRegistry registry;
            if (expressionSyntax is TypeOrFunctionReferenceBindingParserNode typeOrFunction)
                expressionSyntax = typeOrFunction.ToTypeReference();

            if (expressionSyntax is AssemblyQualifiedNameBindingParserNode assemblyQualifiedName)
            {
                registry = TypeRegistry.DirectivesDefault(compiledAssemblyCache, assemblyQualifiedName.AssemblyName.ToDisplayString());
            }
            else
            {
                registry = TypeRegistry.DirectivesDefault(compiledAssemblyCache);
            }

            var visitor = new ExpressionBuildingVisitor(registry, new MemberExpressionFactory(extensionMethodsCache)) {
                ResolveOnlyTypeName = true,
                Scope = null
            };

            try
            {
                return visitor.Visit(expressionSyntax);
            }
            catch (Exception ex)
            {
                directive.AddError($"{expressionSyntax.ToDisplayString()} is not a valid type or namespace: {ex.Message}");
                return null;
            }
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
