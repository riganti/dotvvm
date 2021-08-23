#nullable enable
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
        public static readonly CodeSymbolicParameter KnockoutContextParameter = new ContextSymbolicParameter(0, "");
        public static readonly CodeSymbolicParameter ParentKnockoutContextParameter = new ContextSymbolicParameter(1, "Parent");
        public static readonly CodeSymbolicParameter KnockoutViewModelParameter = new ViewModelSymbolicParameter(0, "", "$data");
        public static readonly CodeSymbolicParameter ParentKnockoutViewModelParameter = new ViewModelSymbolicParameter(1, "Parent", "$parent");

        public static CodeSymbolicParameter GetKnockoutViewModelParameter(int parentIndex) => parentIndex switch {
            0 => KnockoutViewModelParameter,
            1 => ParentKnockoutViewModelParameter,
            _ => new ViewModelSymbolicParameter(parentIndex, $"Parent{parentIndex}", null)
        };
        public static CodeSymbolicParameter GetKnockoutContextParameter(int parentIndex) => parentIndex switch {
            0 => KnockoutContextParameter,
            1 => ParentKnockoutContextParameter,
            _ => new ContextSymbolicParameter(parentIndex, $"Parent{parentIndex}")
        };

        public sealed class ViewModelSymbolicParameter: CodeSymbolicParameter
        {
            internal ViewModelSymbolicParameter(int parentIndex, string description, string? member): base(
                $"JavascriptTranslator.{description}KnockoutViewModelParameter",
                new CodeParameterAssignment(
                    (member is null ? KnockoutContextParameter.ToExpression().Member("$parents").Indexer(new JsLiteral(parentIndex - 1))
                                    : KnockoutContextParameter.ToExpression().Member(member)).FormatParametrizedScript(),
                    isGlobalContext: parentIndex == 0
                )
            )
            {
                this.ParentIndex = parentIndex;
            }
            public int ParentIndex { get; }
        }
        public sealed class ContextSymbolicParameter: CodeSymbolicParameter
        {
            internal ContextSymbolicParameter(int parentIndex, string description): base(
                $"JavascriptTranslator.{description}KnockoutContextParameter",
                new CodeParameterAssignment(
                    GetKnockoutContext(parentIndex).FormatParametrizedScript(),
                    isGlobalContext: parentIndex == 0
                )
            )
            {
                this.ParentIndex = parentIndex;
            }
            public int ParentIndex { get; }

            static JsExpression GetKnockoutContext(int dataContextLevel)
            {
                if (dataContextLevel == 0)
                    return new JsIdentifierExpression("$context");

                JsExpression currentContext = KnockoutContextParameter.ToExpression();
                for (int i = 0; i < dataContextLevel; i++) currentContext = currentContext.Member("$parentContext");

                return currentContext;
            }
        }


        private readonly IViewModelSerializationMapper mapper;

        public IJavascriptMethodTranslator DefaultMethodTranslator { get; }
        public JavascriptTranslator(IOptions<JavascriptTranslatorConfiguration> config, IViewModelSerializationMapper serializationMapper)
        {
            this.DefaultMethodTranslator = config.Value;
            this.mapper = serializationMapper;
        }
        public JavascriptTranslator(IJavascriptMethodTranslator config, IViewModelSerializationMapper serializationMapper)
        {
            this.DefaultMethodTranslator = config;
            this.mapper = serializationMapper;
        }

        public JsExpression TryTranslateMethodCall(Expression context, Expression[] arguments, MethodInfo method, DataContextStack dataContext)
        {
            return new JavascriptTranslationVisitor(dataContext, DefaultMethodTranslator).TryTranslateMethodCall(method, context, arguments);
        }

        public JsViewModelPropertyAdjuster AdjustingVisitor(bool preferUsingState)
        {
            return new JsViewModelPropertyAdjuster(mapper, preferUsingState);
        }

        public JsExpression CompileToJavascript(Expression binding, DataContextStack dataContext, bool preferUsingState = false, bool isRootAsync = false)
        {
            var translator = new JavascriptTranslationVisitor(dataContext, DefaultMethodTranslator);
            var script = new JsParenthesizedExpression(translator.Translate(binding));
            script.AcceptVisitor(AdjustingVisitor(preferUsingState));
            script.AcceptVisitor(new PromiseAwaitingVisitor(isRootAsync));
            return script.Expression.Detach();
        }

        public static ParametrizedCode AdjustKnockoutScriptContext(ParametrizedCode expression, int dataContextLevel)
        {
            if (dataContextLevel == 0) return expression;
            return expression.AssignParameters(o =>
                o is ViewModelSymbolicParameter vm ? GetKnockoutViewModelParameter(vm.ParentIndex + dataContextLevel).ToParametrizedCode() :
                o is ContextSymbolicParameter context ? GetKnockoutContextParameter(context.ParentIndex + dataContextLevel).ToParametrizedCode() :
                default
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
                               o == KnockoutViewModelParameter ? CodeParameterAssignment.FromIdentifier("$data") :
                               default);
        }

        /// <summary>
        /// Get's Javascript code that can be executed inside knockout data-bind attribute.
        /// </summary>
        public static string FormatKnockoutScript(ParametrizedCode expression, ParametrizedCode contextVariable, ParametrizedCode? dataVariable = null)
        {
            return expression
                .ToString(o => o == KnockoutContextParameter ? contextVariable :
                               o == KnockoutViewModelParameter ? dataVariable :
                               throw new Exception());
        }
    }

    public class ViewModelInfoAnnotation : IEquatable<ViewModelInfoAnnotation>
    {
        public Type Type { get; set; }
        public bool IsControl { get; set; }
        public BindingExtensionParameter? ExtensionParameter { get; set; }

        public ViewModelSerializationMap? SerializationMap { get; set; }
        public bool? ContainsObservables { get; set; }

        public bool Equals(ViewModelInfoAnnotation other) =>
            Type == other.Type &&
            IsControl == other.IsControl &&
            ExtensionParameter == other.ExtensionParameter &&
            ContainsObservables == other.ContainsObservables;

        public override bool Equals(object obj) => obj is ViewModelInfoAnnotation obj2 && this.Equals(obj2);

        public override int GetHashCode() => (Type, ExtensionParameter, IsControl, ContainsObservables).GetHashCode();

        public ViewModelInfoAnnotation(Type type, bool isControl = false, BindingExtensionParameter? extensionParameter = null, bool? containsObservables = null)
        {
            this.Type = type;
            this.IsControl = isControl;
            this.ExtensionParameter = extensionParameter;
            this.ContainsObservables = containsObservables;
        }
    }

    /// <summary> Marks that the expression is essentially a member access on the target. We use this to keep track which objects have observables and which don't. </summary>
    public class VMPropertyInfoAnnotation
    {
        public VMPropertyInfoAnnotation(MemberInfo memberInfo, Type? resultType = null, ViewModelPropertyMap? serializationMap = null)
        {
            ResultType = resultType ?? memberInfo.GetResultType();
            MemberInfo = memberInfo;
            SerializationMap = serializationMap;
        }

        public VMPropertyInfoAnnotation(Type resultType)
        {
            ResultType = resultType;
        }

        public Type ResultType { get; }
        public MemberInfo? MemberInfo { get; }
        public ViewModelPropertyMap? SerializationMap { get; set; }
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
