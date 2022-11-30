using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.Binding;
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
        /// <summary> Parameter representing the current knockout context (`$context`) </summary>
        public static readonly ContextSymbolicParameter KnockoutContextParameter = new ContextSymbolicParameter(0, "");
        /// <summary> Parameter representing the parent knockout context (`$parentContext`) </summary>
        public static readonly ContextSymbolicParameter ParentKnockoutContextParameter = new ContextSymbolicParameter(1, "Parent");
        /// <summary> Parameter representing the current view model (`$data`). The view model itself is not an ko.observable, but all it's properties are. </summary>
        public static readonly ViewModelSymbolicParameter KnockoutViewModelParameter = new ViewModelSymbolicParameter(0, "", false);
        /// <summary> Parameter representing the current view model in ko.observable (`$rawData`). </summary>
        public static readonly ViewModelSymbolicParameter KnockoutViewModelObservableParameter = new ViewModelSymbolicParameter(0, "", true);
        /// <summary> Parameter representing the parent view model (`$parent`). The view model itself is not an ko.observable, but all it's properties are. </summary>
        public static readonly ViewModelSymbolicParameter ParentKnockoutViewModelParameter = new ViewModelSymbolicParameter(1, "Parent", false);
        /// <summary> Gets the HTML element where this binding is being evaluated. In knockout bindings, this translates to `$element`, in command bindings (i.e. JS event handlers), it translates to `this` </summary>
        public static readonly CodeSymbolicParameter CurrentElementParameter = new CodeSymbolicParameter("CurrentElement",
            new CodeParameterAssignment("$element", OperatorPrecedence.Max)
        );

        public static ViewModelSymbolicParameter GetKnockoutViewModelParameter(int parentIndex, bool returnsObservable = false) => (parentIndex, returnsObservable) switch {
            (< 0, _) => throw new ArgumentOutOfRangeException("parentIndex"),
            (0, false) => KnockoutViewModelParameter,
            (0, true) => KnockoutViewModelObservableParameter,
            (1, false) => ParentKnockoutViewModelParameter,
            _ => new ViewModelSymbolicParameter(parentIndex, $"Parent{parentIndex}", returnsObservable)
        };
        public static ContextSymbolicParameter GetKnockoutContextParameter(int parentIndex) => parentIndex switch {
            < 0 => throw new ArgumentOutOfRangeException("parentIndex"),
            0 => KnockoutContextParameter,
            1 => ParentKnockoutContextParameter,
            _ => new ContextSymbolicParameter(parentIndex, $"Parent{parentIndex}")
        };

        public sealed class ViewModelSymbolicParameter: CodeSymbolicParameter
        {
            internal ViewModelSymbolicParameter(int parentIndex, string description, bool returnObservable): base(
                $"JavascriptTranslator.{description}KnockoutViewModelParameter",
                new CodeParameterAssignment(
                    GetDefaultAssignment(parentIndex, returnObservable),
                    isGlobalContext: parentIndex == 0
                )
            )
            {
                this.ParentIndex = parentIndex;
                this.ReturnObservable = returnObservable;
            }
            /// <summary> Index in the knockout data context hierarchy. `ParentIndex == 0` means $data, ... </summary>
            public int ParentIndex { get; }
            /// <summary> If the expression should return knockout observable. For `ReturnObservable == true` and `ParentIndex == 0`, it outputs `$rawData`, for false it outputs `$data`. </summary>
            public bool ReturnObservable { get; }

            internal static ParametrizedCode GetDefaultAssignment(int parentIndex, bool returnObservable)
            {
                return (parentIndex, returnObservable) switch {
                    (0, _) => KnockoutContextParameter.ToExpression().Member(returnObservable ? "$rawData" : "$data").FormatParametrizedScript(),
                    (1, false) => KnockoutContextParameter.ToExpression().Member("$parent").FormatParametrizedScript(),
                    (_, false) => KnockoutContextParameter.ToExpression().Member("$parents").Indexer(new JsLiteral(parentIndex - 1)).FormatParametrizedScript(),
                    (_, true) => GetKnockoutContextParameter(parentIndex).ToExpression().Member("$rawData").FormatParametrizedScript(),
                };
            }

            public ViewModelSymbolicParameter WithIndex(int index) =>
                index == ParentIndex ? this : JavascriptTranslator.GetKnockoutViewModelParameter(index, ReturnObservable);
            public ViewModelSymbolicParameter WithReturnsObservable(bool returnsObservable) =>
                returnsObservable == ReturnObservable ? this : JavascriptTranslator.GetKnockoutViewModelParameter(ParentIndex, returnsObservable);
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

        public JsExpression? TryTranslateMethodCall(Expression? context, Expression[] arguments, MethodInfo? method, DataContextStack dataContext)
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
                o == CommandBindingExpression.OptionalKnockoutContextParameter ? GetKnockoutContextParameter(dataContextLevel).ToParametrizedCode() :
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

        public bool Equals(ViewModelInfoAnnotation? other) =>
            object.ReferenceEquals(this, other) ||
            other is not null &&
            Type == other.Type &&
            IsControl == other.IsControl &&
            ExtensionParameter == other.ExtensionParameter &&
            ContainsObservables == other.ContainsObservables;

        public override bool Equals(object? obj) => obj is ViewModelInfoAnnotation obj2 && this.Equals(obj2);

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
        public VMPropertyInfoAnnotation(MemberInfo? memberInfo, Type? resultType = null, ViewModelPropertyMap? serializationMap = null)
        {
            ResultType = resultType ?? memberInfo!.GetResultType();
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

        public static VMPropertyInfoAnnotation FromDotvvmProperty(DotvvmProperty p) =>
            p.PropertyInfo is null ? new VMPropertyInfoAnnotation(p.PropertyType)
                                   : new VMPropertyInfoAnnotation(p.PropertyInfo, p.PropertyType);
    }

    public class JavascriptTranslatorConfiguration: IJavascriptMethodTranslator
    {
        public List<IJavascriptMethodTranslator> Translators { get; } = new List<IJavascriptMethodTranslator>();
        public JavascriptTranslatableMethodCollection MethodCollection { get; }

        public JavascriptTranslatorConfiguration()
        {
            Translators.Add(MethodCollection = new JavascriptTranslatableMethodCollection());
            Translators.Add(new DelegateInvokeMethodTranslator());
            Translators.Add(new CsharpViewModuleMethodTranslator());
        }

        public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method) =>
            Translators.Select(t => t.TryTranslateCall(context, arguments, method)).FirstOrDefault(d => d != null);
    }
}
