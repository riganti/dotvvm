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

        public IAbstractTreeRoot BuildTreeRoot(IControlTreeResolver controlTreeResolver, IControlResolverMetadata metadata, DothtmlRootNode node, IDataContextStack dataContext, IReadOnlyDictionary<string, IReadOnlyList<IAbstractDirective>> directives)
        {
            return new ResolvedTreeRoot((ControlResolverMetadata)metadata, node, (DataContextStack)dataContext, directives);
        }

        public IAbstractControl BuildControl(IControlResolverMetadata metadata, DothtmlNode node, IDataContextStack dataContext)
        {
            return new ResolvedControl((ControlResolverMetadata)metadata, node, (DataContextStack)dataContext);
        }

        public IAbstractBinding BuildBinding(BindingParserOptions bindingOptions, IDataContextStack dataContext, DothtmlBindingNode node, ITypeDescriptor resultType = null, Exception parsingError=null,  object customData = null)
        {
            return new ResolvedBinding()
            {
                BindingType = bindingOptions.BindingType,
                Value = node.Value,
                Expression = (Expression)customData,
                DataContextTypeStack = (DataContextStack)dataContext,
                ParsingError = parsingError,
                DothtmlNode = node,
                ResultType = resultType
            };
        }

        public IAbstractPropertyBinding BuildPropertyBinding(IPropertyDescriptor property, IAbstractBinding binding, DothtmlAttributeNode sourceAttribute)
        {
            return new ResolvedPropertyBinding((DotvvmProperty)property, (ResolvedBinding)binding) { DothtmlNode = sourceAttribute };
        }

        public IAbstractPropertyControl BuildPropertyControl(IPropertyDescriptor property, IAbstractControl control, DothtmlElementNode wrapperElement)
        {
            return new ResolvedPropertyControl((DotvvmProperty)property, (ResolvedControl)control) { DothtmlNode = wrapperElement };
        }

        public IAbstractPropertyControlCollection BuildPropertyControlCollection(IPropertyDescriptor property, IEnumerable<IAbstractControl> controls, DothtmlElementNode wrapperElement)
        {
            return new ResolvedPropertyControlCollection((DotvvmProperty)property, controls.Cast<ResolvedControl>().ToList()) { DothtmlNode = wrapperElement };
        }

        public IAbstractPropertyTemplate BuildPropertyTemplate(IPropertyDescriptor property, IEnumerable<IAbstractControl> templateControls, DothtmlElementNode wrapperElement)
        {
            return new ResolvedPropertyTemplate((DotvvmProperty)property, templateControls.Cast<ResolvedControl>().ToList()) { DothtmlNode = wrapperElement };
        }

        public IAbstractPropertyValue BuildPropertyValue(IPropertyDescriptor property, object value, DothtmlNode sourceNode)
        {
            return new ResolvedPropertyValue((DotvvmProperty)property, value) { DothtmlNode = sourceNode };
        }

        public IAbstractDirective BuildDirective(DothtmlDirectiveNode node)
        {
            return new ResolvedDirective() { DothtmlNode = node };
        }

        public IAbstractHtmlAttributeValue BuildHtmlAttributeValue (string name, string value, DothtmlAttributeNode dothtmlNode)
        {
            return new ResolvedHtmlAttributeValue(name, value) { DothtmlNode = dothtmlNode };
        }

        public IAbstractHtmlAttributeBinding BuildHtmlAttributeBinding(string name, IAbstractBinding binding, DothtmlAttributeNode dothtmlNode)
        {
            return new ResolvedHtmlAttributeBinding(name, (ResolvedBinding)binding) { DothtmlNode = dothtmlNode };
        }

        public void SetHtmlAttribute(IAbstractControl control, IAbstractHtmlAttributeSetter attributeSetter)
        {
            ((ResolvedControl)control).SetHtmlAttribute((ResolvedHtmlAttributeSetter)attributeSetter);
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