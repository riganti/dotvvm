#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedPropertyDeclarationDirective : ResolvedDirective, IAbstractPropertyDeclarationDirective
    {
        public SimpleNameBindingParserNode NameSyntax { get; }
        public TypeReferenceBindingParserNode PropertyTypeSyntax { get; }
        public BindingParserNode? InitializerSyntax { get; }
        public ITypeDescriptor? PropertyType { get; set; }
        public ITypeDescriptor? DeclaringType { get; set; }
        public object? InitialValue { get; }
        public IList<IAbstractDirectiveAttributeReference> Attributes { get; }
        public IList<object> AttributeInstances { get; }
        public object[] GetCustomAttributes(Type attributeType, bool inherit) => GetCustomAttributes(inherit).Where(attributeType.IsInstanceOfType).ToArray();
        public object[] GetCustomAttributes(bool inherit) => AttributeInstances.ToArray();
        public bool IsDefined(Type attributeType, bool inherit) => GetCustomAttributes(attributeType, inherit).Any();

        public ResolvedPropertyDeclarationDirective(
            DirectiveCompilationService service,
            DothtmlDirectiveNode dothtmlDirective,
            SimpleNameBindingParserNode nameSyntax,
            TypeReferenceBindingParserNode typeSyntax,
            BindingParserNode? initializerSyntax,
            IList<IAbstractDirectiveAttributeReference> attributes,
            ImmutableList<NamespaceImport> imports)
            : base(dothtmlDirective)
        {
            NameSyntax = nameSyntax;
            PropertyTypeSyntax = typeSyntax;
            InitializerSyntax = initializerSyntax;
            Attributes = attributes;

            var propertyTypeDescriptor = service.ResolveType(dothtmlDirective, typeSyntax, imports);

            if (propertyTypeDescriptor == null)
            {
                dothtmlDirective.AddError($"Could not resolve type {typeSyntax.ToDisplayString()}.");
            }

            PropertyType = propertyTypeDescriptor;

            //Chack that I am not asigning incompatible types 
            InitialValue = service.ResolvePropertyInitializer(dothtmlDirective, propertyTypeDescriptor?.Type, initializerSyntax, imports);

            AttributeInstances = InstantiateAttributes( dothtmlDirective, attributes).ToList();
        }

        private IEnumerable<object> InstantiateAttributes(DothtmlDirectiveNode dothtmlDirective, IList<IAbstractDirectiveAttributeReference> resolvedAttributes)
        {
            var attributePropertiesByType = resolvedAttributes
                .GroupBy(
                a => a.Type?.FullName,
                a => a,
                (name, attributes) => {

                    var attributeType = (attributes.First().Type as ResolvedTypeDescriptor)?.Type;
                    var properties = attributes.Select(a => (name: a.NameSyntax.Name, value: a.Initializer.Value));

                    return (attributeType, properties);
                }).ToList();

            foreach (var attribute in attributePropertiesByType)
            {
                if (attribute.attributeType is null) { continue; }

                var attributeInstance = Activator.CreateInstance(attribute.attributeType);

                if (attributeInstance is null)
                {
                    dothtmlDirective.AddError($"Could not create insstance of the attribute {attribute.attributeType}.");
                    continue;
                }

                foreach (var property in attribute.properties)
                {
                    var reflectedProperty = attribute.attributeType.GetProperty(property.name);

                    if (reflectedProperty is null)
                    {
                        dothtmlDirective.AddError($"Could not find property {property.name} insstance of the attribute {attribute.attributeType}.");
                        continue;
                    }

                    reflectedProperty.SetValue(attributeInstance, property.value);
                }
                yield return attributeInstance;
            }
        }
    }
}
