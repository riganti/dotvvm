using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Configuration;
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

            // separate method to avoid closure allocation if dataContextLevel == 0
            return shift(expression, dataContextLevel);
            static ParametrizedCode shift(ParametrizedCode expression, int dataContextLevel) =>
                expression.AssignParameters(o =>
                                                                                                                        o is ViewModelSymbolicParameter vm ? GetKnockoutViewModelParameter(vm.ParentIndex + dataContextLevel, vm.ReturnObservable).ToParametrizedCode() :
                                                                                                                        o is ContextSymbolicParameter context ? GetKnockoutContextParameter(context.ParentIndex + dataContextLevel).ToParametrizedCode() :
                                                                                                                        o == CommandBindingExpression.OptionalKnockoutContextParameter ? GetKnockoutContextParameter(dataContextLevel).ToParametrizedCode() :
                                                                                                                        default);
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
            if (dataContextLevel == 0)
            {
                if (allowDataGlobal)
                    return expression.ToDefaultString();
                else
                    return expression.ToString(static o =>
                                o == KnockoutViewModelParameter ? CodeParameterAssignment.FromIdentifier("$data") :
                                default);

            }

            // separate method to avoid closure allocation if dataContextLevel == 0
            return shiftToString(expression, dataContextLevel);

            static string shiftToString(ParametrizedCode expression, int dataContextLevel) =>
                expression.ToString(o => {
                    if (o is ViewModelSymbolicParameter vm)
                    {
                        var p = GetKnockoutViewModelParameter(vm.ParentIndex + dataContextLevel, vm.ReturnObservable).DefaultAssignment;
                        return new(p.Code!.ToDefaultString(), p.Code.OperatorPrecedence);
                    }
                    else if (o is ContextSymbolicParameter context)
                    {
                        var p = GetKnockoutContextParameter(context.ParentIndex + dataContextLevel).DefaultAssignment;
                        return new(p.Code!.ToDefaultString(), p.Code.OperatorPrecedence);
                    }
                    else if (o == CommandBindingExpression.OptionalKnockoutContextParameter)
                    {
                        var p = GetKnockoutContextParameter(dataContextLevel).DefaultAssignment;
                        return new(p.Code!.ToDefaultString(), p.Code.OperatorPrecedence);
                    }
                    else
                    {
                        return default;
                    }
                });
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
        public JsObjectObservableMap ObservableMap { get; set; } = JsObjectObservableMap.Default;
        public bool? ContainsObservables
        {
            get => ObservableMap.ContainsObservables;
            set => ObservableMap = ObservableMap with { ContainsObservables = value };
        }

        public bool Equals(ViewModelInfoAnnotation? other) =>
            object.ReferenceEquals(this, other) ||
            other is not null &&
            Type == other.Type &&
            IsControl == other.IsControl &&
            ExtensionParameter == other.ExtensionParameter &&
            ObservableMap == other.ObservableMap;

        public override bool Equals(object? obj) => obj is ViewModelInfoAnnotation obj2 && this.Equals(obj2);

        public override int GetHashCode() => (Type, ExtensionParameter, IsControl, ContainsObservables).GetHashCode();

        public ViewModelInfoAnnotation(Type type, bool isControl = false, BindingExtensionParameter? extensionParameter = null, bool? containsObservables = null)
        {
            this.Type = type;
            this.IsControl = isControl;
            this.ExtensionParameter = extensionParameter;
            this.ObservableMap = JsObjectObservableMap.FromBool(containsObservables);
        }

        public ViewModelInfoAnnotation(Type type, bool isControl, BindingExtensionParameter? extensionParameter, JsObjectObservableMap? observableMap)
        {
            this.Type = type;
            this.IsControl = isControl;
            this.ExtensionParameter = extensionParameter;
            this.ObservableMap = observableMap ?? JsObjectObservableMap.Default;
        }
    }

    public sealed record JsObjectObservableMap: IEquatable<JsObjectObservableMap>
    {
        /// <summary> The default value for all descendants, unless overriden using the other properties </summary>
        public bool? ContainsObservables { get; set; }

        /// <summary> Default value for all properties. Null means that `ContainsObservables` is assumed to be the same as the parent's </summary>
        public JsObjectObservableMap? DefaultChild { get; set; }

        public Dictionary<string, JsObjectObservableMap>? ChildObjects { get; set; }

        public Dictionary<string, bool>? PropertyIsObservable { get; set; }

        public bool? IsPropertyObservable(string? name) => name is {} && PropertyIsObservable?.TryGetValue(name, out var value) == true ? value : ContainsObservables;
        public bool? IsPropertyObservable(ReadOnlySpan<string?> objectPath)
        {
            if (objectPath.Length == 0) return null;
            var current = this.GetChildObject(objectPath.Slice(0, objectPath.Length - 1));
            return current.IsPropertyObservable(objectPath[objectPath.Length - 1]);
        }

        public JsObjectObservableMap GetChildObject(string? name)
        {
            if (name is {} && ChildObjects?.TryGetValue(name, out var value) == true)
                return value;
            if (DefaultChild is {})
                return DefaultChild;
            return FromBool(ContainsObservables);
        }
        public JsObjectObservableMap GetChildObject(ReadOnlySpan<string?> objectPath)
        {
            var current = this;
            foreach (var name in objectPath)
                current = current.GetChildObject(name);
            return current;
        }

        public JsObjectObservableMap OverrideWith(JsObjectObservableMap? @override)
        {
            if (@override is null || @override == Default || this == @override || this == Default) return this;

            var result = this with { ContainsObservables = @override.ContainsObservables ?? ContainsObservables };

            if (@override.DefaultChild is not null)
            {
                result.DefaultChild = DefaultChild?.OverrideWith(@override.DefaultChild) ?? @override.DefaultChild;
            }

            if (@override.ChildObjects is not null)
            {
                result.ChildObjects = result.ChildObjects is {} ? new(result.ChildObjects) : new();
                foreach (var (key, value) in @override.ChildObjects)
                    result.ChildObjects[key] = result.ChildObjects.TryGetValue(key, out var existing) ? existing.OverrideWith(value) : value;
            }
            if (@override.PropertyIsObservable is not null)
            {
                result.PropertyIsObservable = result.PropertyIsObservable is {} ? new(result.PropertyIsObservable) : new();
                foreach (var (key, value) in @override.PropertyIsObservable)
                    result.PropertyIsObservable[key] = value;
            }
            return result;
        }

        public override string ToString()
        {
            if (this == Default) return "Default";
            if (this == Observables) return "Observables";
            if (this == Plain) return "Plain";

            var sb = new System.Text.StringBuilder().Append("{ ");
            if (ContainsObservables is {})
                sb.Append("ContainsObservables = ").Append(ContainsObservables).Append(", ");
            if (PropertyIsObservable is {Count: > 0})
                sb.Append("Properties = { ").Append(string.Join(", ", PropertyIsObservable.Select(k => $"{k.Key}: {(k.Value ? "observable" : "plain")}"))).Append(" }, ");
            if (DefaultChild is {})
                sb.Append("DefaultChild = ").Append(DefaultChild).Append(", ");
            if (ChildObjects is {Count: > 0})
                sb.Append("ChildObjects = { ").Append(string.Join(", ", ChildObjects.Select(k => $"{k.Key}: {k.Value}"))).Append(" }, ");
            return sb.Append("}").ToString();
        }

        public bool Equals(JsObjectObservableMap? other)
        {
            if (other == (object)this) return true;
            if (other is null) return false;
            return ContainsObservables == other.ContainsObservables &&
                (this.ChildObjects?.Count ?? 0) == (other.ChildObjects?.Count ?? 0) &&
                (this.PropertyIsObservable?.Count ?? 0) == (other.PropertyIsObservable?.Count ?? 0) &&
                DefaultChild == other.DefaultChild &&
                dictEquals(ChildObjects, other.ChildObjects) &&
                dictEquals(PropertyIsObservable, other.PropertyIsObservable);

            static bool dictEquals<K, V>(Dictionary<K, V>? a, Dictionary<K, V>? b) where V: IEquatable<V> where K: notnull
            {
                if (a is null or { Count: 0 } && b is null or { Count: 0 })
                    return true;
                if (a is null || b is null || a.Count != b.Count)
                    return false;

                foreach (var (k, v) in a)
                {
                    if (!b.TryGetValue(k, out var v2) || Equals(v, v2))
                        return false;
                }
                return true;
            }
        }

        public override int GetHashCode()
        {
            var hash = (ContainsObservables, DefaultChild, ChildObjects?.Count, PropertyIsObservable?.Count).GetHashCode();
            if (ChildObjects is not null)
                foreach (var (k, v) in ChildObjects)
                    hash += (k, v).GetHashCode(); // plus is commutative, because dict order should not matter
            if (PropertyIsObservable is not null)
                foreach (var (k, v) in PropertyIsObservable)
                    hash += (k, v).GetHashCode();
            return hash;
        }

        /// <summary> Assume the current preference - i.e. observables in value bindings and plain objects in staticCommands </summary>
        public static readonly JsObjectObservableMap Default = new();
        /// <summary> The object is wrapped in observables all the way down </summary>
        public static readonly JsObjectObservableMap Observables = new() { ContainsObservables = true };
        /// <summary> The object contains plain JS values without knockout observables. </summary>
        public static readonly JsObjectObservableMap Plain = new() { ContainsObservables = false };
        internal static JsObjectObservableMap FromBool(bool? containsObservables) =>
            containsObservables switch {
                true => Observables,
                false => Plain,
                null => Default
            };
    }

    /// <summary>
    /// Marks that the expression is essentially a member access on the target - i.e. an expression which accesses part of a viewmodel. We use this to keep track which objects have observables and which don't.
    /// DotVVM will primarily use this annotate to determine if the current expression is ko.observable or not, based on the target <see cref="ViewModelInfoAnnotation.ContainsObservables" />.
    /// </summary>
    public class VMPropertyInfoAnnotation
    {
        public VMPropertyInfoAnnotation(MemberInfo? memberInfo, Type? resultType = null, ViewModelPropertyMap? serializationMap = null, bool? isObservable = null, ImmutableArray<(JsTreeRole role, int index)>? targetPath = null, ImmutableArray<string?>? objectPath = null)
        {
            ResultType = resultType ?? memberInfo!.GetResultType();
            MemberInfo = memberInfo;
            SerializationMap = serializationMap;
            IsObservable = isObservable;
            TargetPath = targetPath ?? DefaultTargetPath;
            ObjectPath = objectPath ?? ImmutableArray.Create(memberInfo?.Name);
        }

        public VMPropertyInfoAnnotation(Type resultType) : this(null, resultType) { }

        /// <summary> C# type of the expression output value </summary>
        public Type ResultType { get; }
        /// <summary> Property/Field/Indexer or method info this expression represents </summary>
        public MemberInfo? MemberInfo { get; }
        /// <summary> Serialziation map of this property </summary>
        public ViewModelPropertyMap? SerializationMap { get; set; }
        /// <summary> If this property (i.e. the result of this expression) is wrapped in knockout observable. If null, the default based on <see cref="ViewModelInfoAnnotation.ObservableMap" /> will be assumed. </summary>
        public bool? IsObservable { get; }

        /// <summary> By default, it is the first <see cref="JsTreeRoles.TargetExpression" /> expression, but can be configured to be any sub-expression (i.e., the first method argument). </summary>
        public ImmutableArray<(JsTreeRole role, int index)> TargetPath { get; set; }

        /// <summary> Path of the projection in terms of object properties, which is then used assess property observability based on <see cref="JsObjectObservableMap" />. Fields can be <c>null</c>, if the property name is unknown - the <see cref="JsObjectObservableMap.DefaultChild" /> will then be used. If the projection returns a modified version of the original object, empty array should be used as the ObjectPath </summary>
        public ImmutableArray<string?> ObjectPath { get; set; }

        public static VMPropertyInfoAnnotation FromDotvvmProperty(DotvvmProperty p) =>
            p.PropertyInfo is null ? new VMPropertyInfoAnnotation(p.PropertyType)
                                   : new VMPropertyInfoAnnotation(p.PropertyInfo, p.PropertyType);

        /// <summary> This expression is a projection of the object in the `Target` property. </summary>
        public static ImmutableArray<(JsTreeRole role, int index)> DefaultTargetPath = ImmutableArray.Create(((JsTreeRole)JsTreeRoles.TargetExpression, 0));
        /// <summary> This expression a method invocation on the projected object - i.e. the object is is Target.Target </summary>
        public static ImmutableArray<(JsTreeRole role, int index)> InstanceMethodTargetPath = ImmutableArray.Create(((JsTreeRole)JsTreeRoles.TargetExpression, 0), ((JsTreeRole)JsTreeRoles.TargetExpression, 0));
        /// <summary> The expression a function invocation with the projected object as the first argument - i.e. the object is in Arguments[0] </summary>
        public static ImmutableArray<(JsTreeRole role, int index)> FirstArgumentMethodTargetPath = ImmutableArray.Create(((JsTreeRole)JsTreeRoles.Argument, 0));

        /// <summary> Gets the target expression from the annotated expression. </summary>
        public JsNode? EvaluateTargetPath(JsNode expression)
        {
            JsNode? node = expression;
            foreach (var (role, index) in TargetPath)
            {
                if (index < 0) throw new Exception("Invalid target path index");

                if (node is null)
                {
                    return null;
                }

                node = GetChild(node, role, index);
            }
            return node;

            static JsNode? GetChild(JsNode node, JsTreeRole role, int index)
            {
                foreach (var child in node.Children)
                {
                    if (child.Role == role)
                    {
                        if (index == 0)
                        {
                            return child;
                        }

                        index--;
                    }
                }
                return null;
            }
        }

    }

    public class CompositeJavascriptTranslator: IJavascriptMethodTranslator
    {
        private readonly FreezableList<IJavascriptMethodTranslator> _translators = new();
        public IList<IJavascriptMethodTranslator> Translators => _translators;
        public JsExpression? TryTranslateCall(LazyTranslatedExpression? context, LazyTranslatedExpression[] arguments, MethodInfo method)
        {
            foreach (var t in this._translators)
            {
                if (t.TryTranslateCall(context, arguments, method) is {} result)
                    return result;
            }
            return null;
        }

        public void Freeze()
        {
            _translators.Freeze();
        }
    }

    public class JavascriptTranslatorConfiguration: CompositeJavascriptTranslator
    {
        public JavascriptTranslatableMethodCollection MethodCollection { get; }

        public JavascriptTranslatorConfiguration()
        {
            Translators.Add(MethodCollection = JavascriptTranslatableMethodCollection.CreateDefault());
            Translators.Add(new DelegateInvokeMethodTranslator());
            Translators.Add(new CustomPrimitiveTypesConversionTranslator());
        }
    }
}
