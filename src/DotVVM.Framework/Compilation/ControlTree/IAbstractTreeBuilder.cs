using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IAbstractTreeBuilder
    {
        IAbstractTreeRoot BuildTreeRoot(IControlTreeResolver controlTreeResolver, IControlResolverMetadata metadata, DothtmlRootNode node, IDataContextStack dataContext, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives);

        IAbstractControl BuildControl(IControlResolverMetadata metadata, DothtmlNode node, IDataContextStack dataContext);

        IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, DothtmlBindingNode node, IDataContextStack dataContext, Exception parsingError, ITypeDescriptor resultType, object customData);

        IAbstractDirective BuildDirective(DothtmlDirectiveNode node);

        IAbstractPropertyBinding BuildPropertyBinding(IPropertyDescriptor property, IAbstractBinding binding);

        IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl control);

        IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls);

        IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls);

        IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object value);

        void SetHtmlAttribute(IAbstractControl control, string attributeName, object value);

        void SetProperty(IAbstractControl control, IAbstractPropertySetter setter);

        void AddChildControl(IAbstractContentNode control, IAbstractControl child);

    }
}