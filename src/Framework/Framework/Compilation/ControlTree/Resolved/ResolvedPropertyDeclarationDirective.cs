#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedPropertyDeclarationDirective : ResolvedDirective, IAbstractPropertyDeclarationDirective
    {
        public SimpleNameBindingParserNode NameSyntax { get; }
        public TypeReferenceBindingParserNode PropertyTypeSyntax { get; }
        public ITypeDescriptor? PropertyType { get; set; }
        public ITypeDescriptor? DeclaringType { get; set; }
        public object? InitialValue { get; }
        public IList<IAbstractDirectiveAttributeReference> Attributes { get; }
        public IList<object> AttributeInstances { get; }

        public ResolvedPropertyDeclarationDirective(
            SimpleNameBindingParserNode nameSyntax,
            TypeReferenceBindingParserNode typeSyntax,
            ITypeDescriptor? type,
            object? initialValue,
            IList<IAbstractDirectiveAttributeReference> attributes,
            List<object> attributeInstances)
        {
            NameSyntax = nameSyntax;
            PropertyTypeSyntax = typeSyntax;
            PropertyType = type;
            InitialValue = initialValue;
            Attributes = attributes;
            AttributeInstances = attributeInstances;
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit) => GetCustomAttributes(inherit).Where(attributeType.IsInstanceOfType).ToArray();
        public object[] GetCustomAttributes(bool inherit) => AttributeInstances.ToArray();
        public bool IsDefined(Type attributeType, bool inherit) => GetCustomAttributes(attributeType, inherit).Any();
    }
}
