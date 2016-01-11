using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Runtime.ControlTree.Resolved
{
    public class ResolvedTreeBuilder : TreeBuilderBase, IAbstractTreeBuilder
    {
        protected override IAbstractTreeRoot BuildRootCore(IControlResolverMetadata viewMetadata, DothtmlRootNode root)
        {
            return new ResolvedTreeRoot((ControlResolverMetadata)viewMetadata, root, null);
        }

        public IAbstractControl BuildResolvedControl(IControlResolverMetadata metadata, DothtmlNode node, IDataContextStack dataContext)
        {
            return new ResolvedControl((ControlResolverMetadata)metadata, node, (DataContextStack)dataContext);
        }

        public void SetPropertyBinding(IAbstractControl control, IPropertyDescriptor property, IAbstractBinding binding)
        {
            ((ResolvedControl)control).SetProperty(new ResolvedPropertyBinding((DotvvmProperty)property, (ResolvedBinding)binding));
        }

        public void SetPropertyValue(IAbstractControl control, IPropertyDescriptor property, object value)
        {
            ((ResolvedControl)control).SetProperty((ResolvedPropertyValue)BuildPropertyValue(property, value));
        }

        public void SetHtmlAttribute(IAbstractControl control, string attributeName, object value)
        {
            ((ResolvedControl) control).SetHtmlAttribute(attributeName, value);
        }

        public IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, DothtmlBindingNode node, IDataContextStack context, Exception parsingError, ITypeDescriptor resultType, object customData)
        {
            return new ResolvedBinding()
            {
                BindingType = bindingOptions.BindingType,
                Value = node.Value,
                Expression = (Expression)customData,
                DataContextTypeStack = (DataContextStack)context,
                ParsingError = parsingError,
                BindingNode = node,
                ResultType = resultType
            };
        }

        public void SetProperty(IAbstractControl control, IAbstractPropertySetter setter)
        {
            ((ResolvedControl)control).SetProperty((ResolvedPropertySetter)setter);
        }

        public void AddChildControl(IAbstractContentNode control, IAbstractControl child)
        {
            ((ResolvedContentNode)control).Content.Add((ResolvedControl)child);
        }

        public IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls)
        {
            return new ResolvedPropertyTemplate((DotvvmProperty) property, templateControls.Cast<ResolvedControl>().ToList());
        }

        public IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls)
        {
            return new ResolvedPropertyControlCollection((DotvvmProperty)property, controls.Cast<ResolvedControl>().ToList());
        }

        public IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object value)
        {
            return new ResolvedPropertyValue((DotvvmProperty) property, value);
        }

        public IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl control)
        {
            return new ResolvedPropertyControl((DotvvmProperty)property, (ResolvedControl)control);
        }
    }
}