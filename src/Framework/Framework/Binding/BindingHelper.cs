using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.Binding;

namespace DotVVM.Framework.Binding
{
    public static partial class BindingHelper
    {
        [return: MaybeNull]
        public static T GetProperty<T>(this IBinding binding, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException) => (T)binding.GetProperty(typeof(T), errorMode)!;
        public static T GetProperty<T>(this IBinding binding) => GetProperty<T>(binding, ErrorHandlingMode.ThrowException)!;

        [Obsolete]
        public static string GetKnockoutBindingExpression(this IValueBinding binding) =>
            JavascriptTranslator.FormatKnockoutScript(binding.KnockoutExpression);
        /// <summary>
        /// Gets the javascript translation of the binding adjusted to the `currentControl`s DataContext
        /// </summary>
        public static string GetKnockoutBindingExpression(this IValueBinding binding, DotvvmBindableObject currentControl, bool unwrapped = false) =>
            (unwrapped ? binding.UnwrappedKnockoutExpression : binding.KnockoutExpression)
            .FormatKnockoutScript(currentControl, binding);

        /// <summary>
        /// Gets the javascript translation of the binding adjusted to the `currentControl`s DataContext, returned value is ParametrizedCode, so it can be further adjusted
        /// </summary>
        public static ParametrizedCode GetParametrizedKnockoutExpression(this IValueBinding binding, DotvvmBindableObject currentControl, bool unwrapped = false) =>
            JavascriptTranslator.AdjustKnockoutScriptContext(unwrapped ? binding.UnwrappedKnockoutExpression : binding.KnockoutExpression, dataContextLevel: FindDataContextTarget(binding, currentControl).stepsUp);

        /// <summary>
        /// Adjusts the knockout expression to `currentControl`s DataContext like it was translated in `currentBinding`s context
        /// </summary>
        public static string FormatKnockoutScript(this ParametrizedCode code, DotvvmBindableObject currentControl, IBinding currentBinding) =>
            JavascriptTranslator.FormatKnockoutScript(code, dataContextLevel: FindDataContextTarget(currentBinding, currentControl).stepsUp);

        /// <summary>
        /// Adjusts the knockout expression to `currentControl`s DataContext like it was translated in `currentBinding`s context
        /// </summary>
        public static string FormatKnockoutScript(this ParametrizedCode code, DotvvmBindableObject currentControl, IBinding currentBinding, int additionalDataContextSteps) =>
            JavascriptTranslator.FormatKnockoutScript(code, dataContextLevel: FindDataContextTarget(currentBinding, currentControl).stepsUp + additionalDataContextSteps);

        /// <summary>
        /// Gets Internal.PathFragmentProperty or DataContext.KnockoutExpression. Returns null if none of these is set.
        /// </summary>
        public static string? GetDataContextPathFragment(this DotvvmBindableObject currentControl) =>
            (string?)currentControl.GetValue(Internal.PathFragmentProperty, inherit: false) ??
            (currentControl.GetBinding(DotvvmBindableObject.DataContextProperty, inherit: false) is IValueBinding binding ?
                binding.GetProperty<SimplePathExpressionBindingProperty>()
                .Code.FormatKnockoutScript(currentControl, binding) :
            null);


        // PERF: maybe safe last GetValue's target/binding to ThreadLocal variable, so the path does not have to be traversed twice
        /// <summary>
        /// Finds expected context control of the `binding` and returns (parent index of the correct DataContext, control in the correct context)
        /// </summary>
        public static (int stepsUp, DotvvmBindableObject target) FindDataContextTarget(this IBinding binding, DotvvmBindableObject control)
        {
            if (control == null) throw new InvalidOperationException($"Can not evaluate binding without any dataContext.");
            var bindingContext = binding.GetProperty<DataContextStack>(ErrorHandlingMode.ReturnNull);
            return FindDataContextTarget(control, bindingContext, binding);
        }

        internal static (int stepsUp, DotvvmBindableObject target) FindDataContextTarget(DotvvmBindableObject control, DataContextStack? bindingContext, object? contextObject)
        {
            var controlContext = (DataContextStack?)control.GetValue(Internal.DataContextTypeProperty);
            if (bindingContext == null || controlContext == null || controlContext.Equals(bindingContext)) return (0, control);

            var changes = 0;
            foreach (var a in control.GetAllAncestors(includingThis: true))
            {
                if (bindingContext.Equals(a.GetValue(Internal.DataContextTypeProperty, inherit: false)))
                    return (changes, a);

                if (a.properties.Contains(DotvvmBindableObject.DataContextProperty)) changes++;
            }

            throw new NotSupportedException($"Could not find DataContext space of '{contextObject}'. The DataContextType property of the binding does not correspond to DataContextType of the {control.GetType().Name} not any of its ancestor. Control's context is {controlContext}, binding's context is {bindingContext}.");
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static void ExecUpdateDelegate(this BindingUpdateDelegate func, DotvvmBindableObject contextControl, object? value)
        {
            var dataContexts = GetDataContexts(contextControl);
            //var control = contextControl.GetClosestControlBindingTarget();
            func(dataContexts.ToArray(), contextControl, value);
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static void ExecUpdateDelegate<T>(this BindingUpdateDelegate<T> func, DotvvmBindableObject contextControl, T value)
        {
            var dataContexts = GetDataContexts(contextControl);
            //var control = contextControl.GetClosestControlBindingTarget();
            func(dataContexts.ToArray(), contextControl, value);
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static object? ExecDelegate(this BindingDelegate func, DotvvmBindableObject contextControl)
        {
            var dataContexts = GetDataContexts(contextControl);
            return func(dataContexts.ToArray(), contextControl);
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static T ExecDelegate<T>(this BindingDelegate<T> func, DotvvmBindableObject contextControl)
        {
            var dataContexts = GetDataContexts(contextControl);
            return func(dataContexts.ToArray(), contextControl);
        }

        /// <summary>
        /// Gets all data context on the path to root. Maximum count can be specified by `count`
        /// </summary>
        public static IEnumerable<object?> GetDataContexts(this DotvvmBindableObject contextControl, int count = -1)
        {
            DotvvmBindableObject? c = contextControl;
            while (c != null)
            {
                // PERF: O(h^2) because GetValue calls another GetDataContexts
                if (c.IsPropertySet(DotvvmBindableObject.DataContextProperty, inherit: false))
                {
                    yield return c.GetValue(DotvvmBindableObject.DataContextProperty);
                    count--;
                }

                if (count == 0) yield break;

                c = c.Parent;
            }
        }

        /// <summary>
        /// Finds expected DataContext target in control.Ancestors() and evaluates the `binding.BindingDelegate`.
        /// </summary>
        public static object? Evaluate(this IStaticValueBinding binding, DotvvmBindableObject control)
        {
            return ExecDelegate(
                binding.BindingDelegate,
                FindDataContextTarget(binding, control).target);
        }

        /// <summary>
        /// Finds expected DataContext target in control.Ancestors() and evaluates the `binding.BindingDelegate`.
        /// </summary>
        public static T Evaluate<T>(this IStaticValueBinding<T> binding, DotvvmBindableObject control)
        {
            return ExecDelegate(
                binding.BindingDelegate,
                FindDataContextTarget(binding, control).target);
        }

        /// <summary>
        /// Writes the value to binding - bound viewModel property is updated. May throw an exception when binding does not support assignment.
        /// </summary>
        public static void UpdateSource(this IUpdatableValueBinding binding, object? value, DotvvmBindableObject control)
        {
            ExecUpdateDelegate(
                binding.UpdateDelegate,
                FindDataContextTarget(binding, control).target,
                value);
        }

        /// <summary>
        /// Writes the value to binding - bound viewModel property is updated. May throw an exception when binding does not support assignment.
        /// </summary>
        public static void UpdateSource<T>(this IUpdatableValueBinding<T> binding, T value, DotvvmBindableObject control)
        {
            ExecUpdateDelegate(
                binding.UpdateDelegate,
                FindDataContextTarget(binding, control).target,
                value);
        }

        /// <summary>
        /// Finds expected DataContext and gets the delegate from command binding.
        /// </summary>
        public static Delegate GetCommandDelegate(this ICommandBinding binding, DotvvmBindableObject control)
        {
            return (Delegate)ExecDelegate(
                binding.BindingDelegate,
                FindDataContextTarget(binding, control).target).NotNull();
        }

        /// <summary>
        /// Finds expected DataContext and gets the delegate from command binding.
        /// </summary>
        public static T GetCommandDelegate<T>(this ICommandBinding<T> binding, DotvvmBindableObject control)
        {
            return ExecDelegate(
                binding.BindingDelegate,
                FindDataContextTarget(binding, control).target);
        }

        /// <summary>
        /// Finds expected DataContext, gets the delegate from command binding and evaluates it with `args`
        /// </summary>
        public static object? Evaluate(this ICommandBinding binding, DotvvmBindableObject control, params Func<Type, object>[] args)
        {
            var action = binding.GetCommandDelegate(control);
            if (action is Command command) return command();
            if (action is Action actionDelegate) { actionDelegate(); return null; }

            var parameters = action.GetType().GetMethod("Invoke")!.GetParameters();
            var evaluatedArgs = args.Zip(parameters, (a, p) => a(p.ParameterType)).ToArray();
            return action.DynamicInvoke(evaluatedArgs);
        }

        /// <summary>
        /// Gets DataContext-adjusted javascript that can be used for command invocation.
        /// </summary>
        public static ParametrizedCode GetParametrizedCommandJavascript(this ICommandBinding binding, DotvvmBindableObject control) =>
            JavascriptTranslator.AdjustKnockoutScriptContext(binding.CommandJavascript,
                dataContextLevel: FindDataContextTarget(binding, control).stepsUp);

        public static object? GetBindingValue(this IBinding binding, DotvvmBindableObject control)
        {
            if (binding is IStaticValueBinding valueBinding)
            {
                return valueBinding.Evaluate(control);
            }
            else if (binding is ICommandBinding command)
            {
                return command.GetCommandDelegate(control);
            }
            else throw new NotSupportedException();
        }

        /// <summary>
        /// Creates new `TBinding` with the original DataContextStack, LocationInfo, AdditionalResolvers and BindingCompilationService. 
        /// </summary>
        public static TBinding DeriveBinding<TBinding>(this TBinding binding, DataContextStack newDataContext, Expression expression, params object?[] properties)
            where TBinding : IBinding
        {
            return binding.DeriveBinding(
                properties.Concat(new object[] {
                    newDataContext,
                    new ParsedExpressionBindingProperty(expression),
                    new CastedExpressionBindingProperty(expression),
                    new ExpectedTypeBindingProperty(expression.Type),
                    new ResultTypeBindingProperty(expression.Type)
                }).ToArray()
            );
        }

        /// <summary>
        /// Creates new `TBinding` with the original DataContextStack, LocationInfo, AdditionalResolvers and BindingCompilationService. 
        /// </summary>
        public static TBinding DeriveBinding<TBinding>(this TBinding binding, Expression expression, params object?[] properties)
            where TBinding : IBinding
        {
            return binding.DeriveBinding(
                properties.Concat(new object[] {
                    new ParsedExpressionBindingProperty(expression),
                    new CastedExpressionBindingProperty(expression),
                    new ExpectedTypeBindingProperty(expression.Type),
                    new ResultTypeBindingProperty(expression.Type)
                }).ToArray()
            );
        }

        /// <summary>
        /// Creates new `TBinding` with the original DataContextStack, LocationInfo, AdditionalResolvers and BindingCompilationService. 
        /// </summary>
        public static TBinding DeriveBinding<TBinding>(this TBinding binding, params object?[] properties)
            where TBinding : IBinding
        {
            object?[] getContextProperties(IBinding b) =>
                new object?[] {
                    b.GetProperty<DataContextStack>(ErrorHandlingMode.ReturnNull),
                    b.GetProperty<BindingResolverCollection>(ErrorHandlingMode.ReturnNull),
                    b.GetProperty<BindingCompilationRequirementsAttribute>(ErrorHandlingMode.ReturnNull)?.ClearRequirements(),
                    b.GetProperty<BindingErrorReporterProperty>(ErrorHandlingMode.ReturnNull),
                    b.GetProperty<LocationInfoBindingProperty>(ErrorHandlingMode.ReturnNull)
                };
            var service = binding.GetProperty<BindingCompilationService>();
            var bindingType = binding.GetType();
            if (bindingType.IsGenericType)
                bindingType = bindingType.GetGenericTypeDefinition();
            return (TBinding)service.CreateBinding(bindingType, getContextProperties(binding).Concat(properties).ToArray());
        }

        /// <summary>
        /// Caches all function evaluations in the closure based on parameter. TParam should be immutable, as it is used as Dictionary key.
        /// It thread-safe.
        /// </summary>
        public static Func<TParam, TResult> Cache<TParam, TResult>(this Func<TParam, TResult> func)
            where TParam: notnull
        {
            var cache = new ConcurrentDictionary<TParam, TResult>();
            return f => cache.GetOrAdd(f, func);
        }

        public static IValueBinding GetThisBinding(this DotvvmBindableObject obj)
        {
            var dataContext = obj.GetValueBinding(DotvvmBindableObject.DataContextProperty);
            return (IValueBinding)dataContext!.GetProperty<ThisBindingProperty>().binding;
        }

        private static readonly ConditionalWeakTable<Expression, BindingParameterAnnotation> _expressionAnnotations =
            new ConditionalWeakTable<Expression, BindingParameterAnnotation>();
        public static TExpression AddParameterAnnotation<TExpression>(this TExpression expr, BindingParameterAnnotation annotation)
            where TExpression : Expression
        {
            _expressionAnnotations.Add(expr, annotation);
            return expr;
        }

        public static BindingParameterAnnotation? GetParameterAnnotation(this Expression expr) =>
            _expressionAnnotations.TryGetValue(expr, out var annotation) ? annotation : null;

        public static void SetDataContextTypeFromDataSource(this DotvvmBindableObject obj, IBinding dataSourceBinding) =>
            obj.SetDataContextType(dataSourceBinding.GetProperty<CollectionElementDataContextBindingProperty>().DataContext);


        /// <summary> Return the expected data context type for this property. Returns null if the type is unknown. </summary>
        public static DataContextStack? GetDataContextType(this DotvvmProperty property, DotvvmBindableObject obj)
        {
            var dataContextType = obj.GetDataContextType();

            if (dataContextType == null)
            {
                return null;
            }

            if (property.DataContextManipulationAttribute != null)
            {
                return property.DataContextManipulationAttribute.ChangeStackForChildren(dataContextType, obj, property, (parent, changeType) => DataContextStack.Create(changeType, parent));
            }

            if (property.DataContextChangeAttributes == null || property.DataContextChangeAttributes.Length == 0)
            {
                return dataContextType;
            }

            var (childType, extensionParameters) = ApplyDataContextChange(dataContextType, property.DataContextChangeAttributes, obj, property);

            if (childType is null) return null; // childType is null in case there is some error in processing (e.g. enumerable was expected).
            else return DataContextStack.Create(childType, dataContextType, extensionParameters: extensionParameters.ToArray());
        }

        /// <summary> Return the expected data context type for this property. Returns null if the type is unknown. </summary>
        public static DataContextStack GetDataContextType(this DotvvmProperty property, ResolvedControl obj)
        {
            var dataContextType = obj.DataContextTypeStack;

            if (property.DataContextManipulationAttribute != null)
            {
                return (DataContextStack)property.DataContextManipulationAttribute.ChangeStackForChildren(
                    dataContextType, obj, property,
                    (parent, changeType) => DataContextStack.Create(ResolvedTypeDescriptor.ToSystemType(changeType), (DataContextStack)parent));
            }

            if (property.DataContextChangeAttributes == null || property.DataContextChangeAttributes.Length == 0)
            {
                return dataContextType;
            }

            var (childType, extensionParameters) = ApplyDataContextChange(dataContextType, property.DataContextChangeAttributes, obj, property);

            if (childType is null)
                childType = typeof(UnknownTypeSentinel);
            
            return DataContextStack.Create(childType, dataContextType, extensionParameters: extensionParameters.ToArray());
        }

        public static (Type? type, List<BindingExtensionParameter> extensionParameters) ApplyDataContextChange(DataContextStack dataContext, DataContextChangeAttribute[] attributes, ResolvedControl control, DotvvmProperty? property)
        {
            var type = ResolvedTypeDescriptor.Create(dataContext.DataContextType);
            var extensionParameters = new List<BindingExtensionParameter>();
            foreach (var attribute in attributes.OrderBy(a => a.Order))
            {
                if (type == null) break;
                extensionParameters.AddRange(attribute.GetExtensionParameters(type));
                type = attribute.GetChildDataContextType(type, dataContext, control, property);
            }
            return (ResolvedTypeDescriptor.ToSystemType(type), extensionParameters);
        }


        private static (Type? childType, List<BindingExtensionParameter> extensionParameters) ApplyDataContextChange(DataContextStack dataContextType, DataContextChangeAttribute[] attributes, DotvvmBindableObject obj, DotvvmProperty property)
        {
            Type? type = dataContextType.DataContextType;
            var extensionParameters = new List<BindingExtensionParameter>();

            foreach (var attribute in attributes.OrderBy(a => a.Order))
            {
                if (type == null) break;
                extensionParameters.AddRange(attribute.GetExtensionParameters(new ResolvedTypeDescriptor(type)));
                type = attribute.GetChildDataContextType(type, dataContextType, obj, property);
            }

            return (type, extensionParameters);
        }

        /// <summary>
        /// Annotates `_this`, `_parent`, `_root` parameters with BindingParameterAnnotation indicating their DataContext
        /// </summary>
        public static Expression AnnotateStandardContextParams(Expression expr, DataContextStack dataContext) =>
            new ParameterAnnotatingVisitor(dataContext).Visit(expr);

        class ParameterAnnotatingVisitor : ExpressionVisitor
        {
            public readonly DataContextStack DataContext;

            public ParameterAnnotatingVisitor(DataContextStack dataContext)
            {
                this.DataContext = dataContext;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node.GetParameterAnnotation() != null) return node;
                if (node.Name == "_this") return node.AddParameterAnnotation(new BindingParameterAnnotation(DataContext));
                else if (node.Name == "_parent") return node.AddParameterAnnotation(new BindingParameterAnnotation(DataContext.Parent));
                else if (node.Name == "_root") return node.AddParameterAnnotation(new BindingParameterAnnotation(DataContext.EnumerableItems().Last()));
                else if (node.Name.StartsWith("_parent", StringComparison.Ordinal) && int.TryParse(node.Name.Substring("_parent".Length), out int index))
                    return node.AddParameterAnnotation(new BindingParameterAnnotation(DataContext.EnumerableItems().ElementAt(index)));
                return base.VisitParameter(node);
            }
        }

        public static BindingDelegate<T> ToGeneric<T>(this BindingDelegate d) => (a, b) => (T)d(a, b)!;
        public static BindingUpdateDelegate<T> ToGeneric<T>(this BindingUpdateDelegate d) => (a, b, c) => d(a, b, c);
    }


    public class BindingParameterAnnotation
    {
        public readonly DataContextStack? DataContext;
        public readonly BindingExtensionParameter? ExtensionParameter;

        public BindingParameterAnnotation(DataContextStack? context = null, BindingExtensionParameter? extensionParameter = null)
        {
            this.DataContext = context;
            this.ExtensionParameter = extensionParameter;
        }
    }
}
