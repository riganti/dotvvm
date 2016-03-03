using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedTreeBuilder : IAbstractTreeBuilder
    {

        public IAbstractTreeRoot BuildTreeRoot(IControlTreeResolver controlTreeResolver, IControlResolverMetadata metadata, DothtmlRootNode node, IDataContextStack dataContext)
        {
            return new ResolvedTreeRoot((ControlResolverMetadata)metadata, node, (DataContextStack)dataContext);
        }

        public IAbstractControl BuildControl(IControlResolverMetadata metadata, DothtmlNode node, IDataContextStack dataContext)
        {
            return new ResolvedControl((ControlResolverMetadata)metadata, node, (DataContextStack)dataContext);
        }

        public IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, DothtmlBindingNode node, IDataContextStack dataContext, Exception parsingError, ITypeDescriptor resultType, object customData)
        {
            return new ResolvedBinding()
            {
                BindingType = bindingOptions.BindingType,
                Value = node.Value,
                Expression = (Expression)customData,
                DataContextTypeStack = (DataContextStack)dataContext,
                ParsingError = parsingError,
                BindingNode = node,
                ResultType = resultType
            };
        }

        public IAbstractPropertyBinding BuildPropertyBinding(IPropertyDescriptor property, IAbstractBinding binding)
        {
            return new ResolvedPropertyBinding((DotvvmProperty)property, (ResolvedBinding)binding);
        }

        public IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl control)
        {
            return new ResolvedPropertyControl((DotvvmProperty)property, (ResolvedControl)control);
        }

        public IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls)
        {
            return new ResolvedPropertyControlCollection((DotvvmProperty)property, controls.Cast<ResolvedControl>().ToList());
        }

        public IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls)
        {
            return new ResolvedPropertyTemplate((DotvvmProperty)property, templateControls.Cast<ResolvedControl>().ToList());
        }

        public IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object value)
        {
            return new ResolvedPropertyValue((DotvvmProperty)property, value);
        }

        public void SetHtmlAttribute(IAbstractControl control, string attributeName, object value)
        {
            ((ResolvedControl) control).SetHtmlAttribute(attributeName, value);
        }

        public void SetProperty(IAbstractControl control, IAbstractPropertySetter setter)
        {
            ((ResolvedControl)control).SetProperty((ResolvedPropertySetter)setter);
        }

        public void AddChildControl(IAbstractContentNode control, IAbstractControl child)
        {
            ((ResolvedContentNode)control).AddChild((ResolvedControl)child);
        }
    }
}