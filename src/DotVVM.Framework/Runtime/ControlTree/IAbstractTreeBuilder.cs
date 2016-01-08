using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractTreeBuilder
    {
        IAbstractTreeRoot BuildRoot(IControlTreeResolver resolver, IControlResolverMetadata viewMetadata, DothtmlRootNode root);

        IAbstractControl BuildResolvedControl(IControlResolverMetadata resolveControl, DothtmlNode node, IDataContextStack dataContext);

        IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, DothtmlBindingNode node, Expression expression, IDataContextStack context, Exception parsingError);

        IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls);

        IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls);

        IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object value);

        IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl control);

        void SetPropertyBinding(IAbstractControl control, IPropertyDescriptor property, IAbstractBinding binding);

        void SetPropertyValue(IAbstractControl control, IPropertyDescriptor property, object value);

        void SetHtmlAttribute(IAbstractControl control, string attributeName, object value);

        void SetProperty(IAbstractControl control, IAbstractPropertySetter setter);

        void AddChildControl(IAbstractContentNode control, IAbstractControl child);

    }
}