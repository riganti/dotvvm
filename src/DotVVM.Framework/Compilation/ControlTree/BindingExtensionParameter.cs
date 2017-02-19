using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public abstract class BindingExtensionParameter
    {
        public string Identifier { get; }
        public bool Inherit { get; }
        public ITypeDescriptor ParameterType { get; }

        public BindingExtensionParameter(string identifier, ITypeDescriptor type, bool inherit)
        {
            this.Identifier = identifier;
            this.ParameterType = type;
            this.Inherit = inherit;
        }

        public abstract Expression GetServerEquivalent(Expression controlParameter);
        public abstract JsExpression GetJsTranslation(JsExpression dataContext);
    }

    public class CurrentMarkupControlExtensionParameter : BindingExtensionParameter
    {
        public CurrentMarkupControlExtensionParameter(ITypeDescriptor controlType) : base("_control", controlType, true)
        {
        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.Convert(ExpressionUtils.Replace((DotvvmBindableObject c) => c.GetClosestControlBindingTarget(), controlParameter), ResolvedTypeDescriptor.ToSystemType(ParameterType));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return dataContext.Member("$control").WithAnnotation(new ViewModelInfoAnnotation(ResolvedTypeDescriptor.ToSystemType(this.ParameterType), isControl: true));
        }
    }

    public class CurrentCollectionIndexExtensionParameter : BindingExtensionParameter
    {
        public CurrentCollectionIndexExtensionParameter() : base("_index", new ResolvedTypeDescriptor(typeof(int)), true)
        {

        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return ExpressionUtils.Replace((DotvvmBindableObject c) => c.GetAllAncestors(true).OfType<DataItemContainer>().First().DataItemIndex.Value, controlParameter);
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsSymbolicParameter(JavascriptTranslator.CurrentIndexParameter);
        }
    }

    public class BindingPageInfoExtensionParameter : BindingExtensionParameter
    {
        public BindingPageInfoExtensionParameter() : base("_page", new ResolvedTypeDescriptor(typeof(BindingPageInfo)), true)
        {

        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.New(typeof(BindingPageInfo));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsObjectExpression(
                new JsObjectProperty(nameof(BindingPageInfo.EvaluatingOnClient), new JsLiteral(true)),
                new JsObjectProperty(nameof(BindingPageInfo.EvaluatingOnServer), new JsLiteral(false)),
                new JsObjectProperty(nameof(BindingPageInfo.IsPostbackRunning), new JsIdentifierExpression("dotvvm").Member("isPostbackRunning").Invoke())
            );
        }
    }
}
