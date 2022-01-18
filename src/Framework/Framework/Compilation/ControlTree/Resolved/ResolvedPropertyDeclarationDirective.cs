#nullable enable

using System;
using System.Collections.Generic;
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
            IList<IAbstractDirectiveAttributeReference> attributes)
            : base(dothtmlDirective)
        {
            NameSyntax = nameSyntax;
            PropertyTypeSyntax = typeSyntax;
            InitializerSyntax = initializerSyntax;
            Attributes = attributes;

            var propertyTypeDescriptor = service.ResolveType(dothtmlDirective, typeSyntax);

            if (propertyTypeDescriptor == null)
            {
                dothtmlDirective.AddError($"Could not resolve type {typeSyntax.ToDisplayString()}.");
            }

            PropertyType = propertyTypeDescriptor;

            //Chack that I am not asigning incompatible types 
            InitialValue = service.ResolvePropertyInitializer(dothtmlDirective, propertyTypeDescriptor?.Type, initializerSyntax);

            AttributeInstances = InstantiateAttributes(attributes).ToList();
        }

        private IEnumerable<object> InstantiateAttributes(IList<IAbstractDirectiveAttributeReference> resolvedAttributes)
        {
            var attributePropertyGrouping = resolvedAttributes.GroupBy(
                a => a.Type.FullName,
                a => a,
                (name, attributes) => {

                    var attributeType = attributes.First().Type.CastTo<ResolvedTypeDescriptor>().Type;
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
    }
}
