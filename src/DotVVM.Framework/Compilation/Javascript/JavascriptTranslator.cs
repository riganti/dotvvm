using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JavascriptTranslator
    {
        public static readonly CodeSymbolicParameter KnockoutContextParameter = new CodeSymbolicParameter("JavascriptTranslator.KnockoutContextParameter", CodeParameterAssignment.FromIdentifier("$context", true));
        public static readonly CodeSymbolicParameter KnockoutViewModelParameter = new CodeSymbolicParameter("JavascriptTranslator.KnockoutViewModelParameter", CodeParameterAssignment.FromIdentifier("$data", true));
        private readonly IViewModelSerializationMapper mapper;

        public IJavascriptMethodTranslator DefaultMethodTranslator { get; }
        public JavascriptTranslator(IOptions<JavascriptTranslatorConfiguration> config, IViewModelSerializationMapper serializationMapper)
        {
            this.DefaultMethodTranslator = config.Value;
            this.mapper = serializationMapper;
        }

        public JsExpression TryTranslateMethodCall(Expression context, Expression[] arguments, MethodInfo method, DataContextStack dataContext)
        {
            return new JavascriptTranslationVisitor(dataContext, DefaultMethodTranslator).TryTranslateMethodCall(method, context, arguments);
        }

        public void AdjustViewModelProperties(JsNode expr)
        {
            expr.AcceptVisitor(new JsViewModelPropertyAdjuster(mapper));
        }

        public JsExpression CompileToJavascript(Expression binding, DataContextStack dataContext)
        {
            var translator = new JavascriptTranslationVisitor(dataContext, DefaultMethodTranslator);
            var script = translator.Translate(binding);
            script.AcceptVisitor(new JsViewModelPropertyAdjuster(mapper));
            return script;
        }

        // public static JsExpression RemoveTopObservables(JsExpression expression)
        // {
        //     foreach (var leaf in expression.GetLeafResultNodes())
        //     {
        //         JsExpression replacement = null;
        //         if (leaf is JsInvocationExpression invocation && invocation.Annotation<ObservableUnwrapInvocationAnnotation>() != null)
        //         {
        //             replacement = invocation.Target;
        //         }
        //         else if (leaf is JsMemberAccessExpression member && member.MemberName == "$data" && member.Target is JsSymbolicParameter par && par.Symbol == JavascriptTranslator.KnockoutContextParameter ||
        //             leaf is JsSymbolicParameter param && param.Symbol == JavascriptTranslator.KnockoutViewModelParameter)
        //         {
        //             replacement = new JsSymbolicParameter(KnockoutContextParameter).Member("$rawData")
        //                 .WithAnnotation(leaf.Annotation<ViewModelInfoAnnotation>());
        //         }

        //         if (replacement != null)
        //         {
        //             if (leaf.Parent == null) expression = replacement;
        //             else leaf.ReplaceWith(replacement);
        //         }
        //     }
        //     return expression;
        // }

        public static (JsExpression context, JsExpression data) GetKnockoutContextParameters(int dataContextLevel)
        {
            JsExpression currentContext = new JsSymbolicParameter(KnockoutContextParameter);
            for (int i = 0; i < dataContextLevel; i++) currentContext = currentContext.Member("$parentContext");

            var currentData = dataContextLevel == 0 ? new JsSymbolicParameter(KnockoutContextParameter).Member("$data") :
                              dataContextLevel == 1 ? new JsSymbolicParameter(KnockoutContextParameter).Member("$parent") :
                              new JsSymbolicParameter(KnockoutContextParameter).Member("$parents").Indexer(new JsLiteral(dataContextLevel - 1));
            return (currentContext, currentData);
        }

        public static ParametrizedCode AdjustKnockoutScriptContext(ParametrizedCode expression, int dataContextLevel)
        {
            if (dataContextLevel == 0) return expression;
            var (contextExpression, dataExpression) = GetKnockoutContextParameters(dataContextLevel);
            var (context, data) = (CodeParameterAssignment.FromExpression(contextExpression), CodeParameterAssignment.FromExpression(dataExpression));
            return expression.AssignParameters(o =>
                o == KnockoutContextParameter ? context :
                o == KnockoutViewModelParameter ? data :
                default(CodeParameterAssignment)
            );
        }

        /// <summary>
        /// Get's Javascript code that can be executed inside knockout data-bind attribute.
        /// </summary>
        public static string FormatKnockoutScript(JsExpression expression, bool allowDataGlobal = true, int dataContextLevel = 0) =>
            FormatKnockoutScript(expression.FormatParametrizedScript(), allowDataGlobal, dataContextLevel);
        /// <summary>
        /// Get's Javascript code that can be executed inside knockout data-bind attribute.
        /// </summary>
        public static string FormatKnockoutScript(ParametrizedCode expression, bool allowDataGlobal = true, int dataContextLevel = 0)
        {
            // TODO(exyi): more symbolic parameters
            var adjusted = AdjustKnockoutScriptContext(expression, dataContextLevel);
            if (allowDataGlobal)
                return adjusted.ToDefaultString();
            else
                return adjusted.ToString(o =>
                               o == KnockoutViewModelParameter ? CodeParameterAssignment.FromIdentifier("$data", allowDataGlobal) :
                               default);
        }

        /// <summary>
        /// Get's Javascript code that can be executed inside knockout data-bind attribute.
        /// </summary>
        public static string FormatKnockoutScript(ParametrizedCode expression, ParametrizedCode contextVariable, ParametrizedCode dataVariable = null)
        {
            if (dataVariable == null) dataVariable = new ParametrizedCode.Builder { contextVariable, ".$data" }.Build(OperatorPrecedence.Max);
            return expression
                .ToString(o => o == KnockoutContextParameter ? new CodeParameterAssignment(contextVariable) :
                               o == KnockoutViewModelParameter ? dataVariable :
                               throw new Exception());
        }
    }

    public class ViewModelInfoAnnotation : IEquatable<ViewModelInfoAnnotation>
    {
        public Type Type { get; set; }
        public bool IsControl { get; set; }
        public BindingExtensionParameter ExtensionParameter { get; set; }

        public ViewModelSerializationMap SerializationMap { get; set; }
        public bool ContainsObservables { get; set; }

        public bool Equals(ViewModelInfoAnnotation other) =>
            Type == other.Type &&
            IsControl == other.IsControl &&
            ExtensionParameter == other.ExtensionParameter &&
            ContainsObservables == other.ContainsObservables;

        public override bool Equals(object obj) => obj is ViewModelInfoAnnotation obj2 && this.Equals(obj2);

        public override int GetHashCode()
        {
            var hash = 69848087;
            hash += Type.GetHashCode();
            hash *= 444_272_593;
            hash += ExtensionParameter.GetHashCode();
            hash *= 444_272_617;
            if (IsControl) hash *= 444_272_629;
            if (ContainsObservables) hash *= 444_272_641;
            return hash;
        }

        public ViewModelInfoAnnotation(Type type, bool isControl = false, BindingExtensionParameter extensionParameter = null, bool containsObservables = true)
        {
            this.Type = type;
            this.IsControl = isControl;
            this.ExtensionParameter = extensionParameter;
            this.ContainsObservables = containsObservables;
        }
    }

    public class VMPropertyInfoAnnotation
    {
        public MemberInfo MemberInfo { get; set; }
        public ViewModelPropertyMap SerializationMap { get; set; }
    }

    public class JavascriptTranslatorConfiguration: IJavascriptMethodTranslator
    {
        public List<IJavascriptMethodTranslator> Translators { get; } = new List<IJavascriptMethodTranslator>();
        public JavascriptTranslatableMethodCollection MethodCollection { get; }

        public JavascriptTranslatorConfiguration()
        {
            Translators.Add(MethodCollection = new JavascriptTranslatableMethodCollection());
            Translators.Add(new EnumToStringMethodTranslator());
            Translators.Add(new DelegateInvokeMethodTranslator());
        }

        public JsExpression TryTranslateCall(LazyTranslatedExpression context, LazyTranslatedExpression[] arguments, MethodInfo method) =>
            Translators.Select(t => t.TryTranslateCall(context, arguments, method)).FirstOrDefault(d => d != null);
    }
}
