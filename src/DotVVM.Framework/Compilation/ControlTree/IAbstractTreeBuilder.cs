using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeBuilder
    {
        IAbstractTreeRoot BuildTreeRoot(IControlTreeResolver controlTreeResolver, IControlResolverMetadata metadata, DothtmlRootNode node, IDataContextStack dataContext, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives);

        IAbstractControl BuildControl(IControlResolverMetadata metadata, DothtmlNode node, IDataContextStack dataContext);

        IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, IDataContextStack dataContext, DothtmlBindingNode node, ITypeDescriptor resultType = null, Exception parsingError = null, object customData = null);
        
        IAbstractDirective BuildDirective(DothtmlDirectiveNode node);

        IAbstractImportDirective BuildImportDirective(DothtmlDirectiveNode node, BindingParserNode aliasSyntax, BindingParserNode nameSyntax);

        IAbstractHtmlAttributeValue BuildHtmlAttributeValue(string Name, string value, DothtmlAttributeNode dothtmlNode);

        IAbstractHtmlAttributeBinding BuildHtmlAttributeBinding(string Name, IAbstractBinding binding, DothtmlAttributeNode dothtmlNode);

        IAbstractPropertyBinding BuildPropertyBinding(IPropertyDescriptor property, IAbstractBinding binding, DothtmlAttributeNode sourceAttributeNode);

        IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl control, DothtmlElementNode wrapperElementNode);

        IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls, DothtmlElementNode wrapperElementNode);

        IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls, DothtmlElementNode wrapperElementNode);

        IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object value, DothtmlNode sourceAttributeNode);

        void SetHtmlAttribute(IAbstractControl control, IAbstractHtmlAttributeSetter attributeSetter);

        void SetProperty(IAbstractControl control, IAbstractPropertySetter setter);

        void AddChildControl(IAbstractContentNode control, IAbstractControl child);

    }
}