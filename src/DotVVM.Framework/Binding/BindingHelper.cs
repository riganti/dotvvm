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

namespace DotVVM.Framework.Binding
{
    public static partial class BindingHelper
    {
        public static T GetProperty<T>(this IBinding binding, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException) => (T)binding.GetProperty(typeof(T), errorMode);

        [Obsolete]
        public static string GetKnockoutBindingExpression(this IValueBinding binding) =>
            JavascriptTranslator.FormatKnockoutScript(binding.KnockoutExpression);
        /// <summary>
        /// Gets the javascript translation of the binding adjusted to the `currentControl`s DataContext
        /// </summary>
        public static string GetKnockoutBindingExpression(this IValueBinding binding, DotvvmBindableObject currentControl, bool unwrapped = false) =>
            (unwrapped ? binding.UnwrapedKnockoutExpression : binding.KnockoutExpression)
            .FormatKnockoutScript(currentControl, binding);

        /// <summary>
        /// Gets the javascript translation of the binding adjusted to the `currentControl`s DataContext, returned value is ParametrizedCode, so it can be further adjusted
        /// </summary>
        public static ParametrizedCode GetParametrizedKnockoutExpression(this IValueBinding binding, DotvvmBindableObject currentControl, bool unwraped = false) =>
            JavascriptTranslator.AdjustKnockoutScriptContext(unwraped ? binding.UnwrapedKnockoutExpression : binding.KnockoutExpression, dataContextLevel: FindDataContextTarget(binding, currentControl).stepsUp);

        /// <summary>
        /// Adjusts the knockout expression to `currentControl`s DataContext like it was translated in `currentBinding`s context
        /// </summary>
        public static string FormatKnockoutScript(this ParametrizedCode code, DotvvmBindableObject currentControl, IBinding currentBinding) =>
            JavascriptTranslator.FormatKnockoutScript(code, dataContextLevel: FindDataContextTarget(currentBinding, currentControl).stepsUp);

        /// <summary>
        /// Gets Internal.PathFragmentProperty or DataContext.KnockoutExpression
        /// </summary>
        public static string GetDataContextPathFragment(this DotvvmBindableObject currentControl) =>
            (string)currentControl.GetValue(Internal.PathFragmentProperty, inherit: false) ??
            (currentControl.GetBinding(DotvvmBindableObject.DataContextProperty, inherit: false) is IValueBinding binding ?
                binding.GetProperty<SimplePathExpressionBindingProperty>(ErrorHandlingMode.ThrowException)
                .Code.FormatKnockoutScript(currentControl, binding) :
            null);


        // PERF: maybe safe last GetValue's target/binding to ThreadLocal variable, so the path does not have to be traversed twice
        /// <summary>
        /// Finds expected context control of the `binding` and returns (parent index of the correct DataContext, control in the corrent context)
        /// </summary>
        public static (int stepsUp, DotvvmBindableObject target) FindDataContextTarget(this IBinding binding, DotvvmBindableObject control)
        {
            if (control == null) throw new InvalidOperationException($"Can not evaluate binding without any dataContext.");
            var controlContext = (DataContextStack)control.GetValue(Internal.DataContextTypeProperty);
            var bindingContext = binding.GetProperty<DataContextStack>(ErrorHandlingMode.ReturnNull);
            if (bindingContext == null || controlContext == null || controlContext.Equals(bindingContext)) return (0, control);

            var changes = 0;
            foreach (var a in control.GetAllAncestors(incudingThis: true))
            {
                if (a.properties.ContainsKey(DotvvmBindableObject.DataContextProperty)) changes++;
                if (bindingContext.Equals(a.GetValue(Internal.DataContextTypeProperty, inherit: false)))
                    return (changes, a);
            }

            throw new NotSupportedException($"Could not find DataContextSpace of binding '{binding}'.");
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static void ExecUpdateDelegate(this CompiledBindingExpression.BindingUpdateDelegate func, DotvvmBindableObject contextControl, object value)
        {
            var dataContexts = GetDataContexts(contextControl);
            //var control = contextControl.GetClosestControlBindingTarget();
            func(dataContexts.ToArray(), contextControl, value);
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static void ExecUpdateDelegate<T>(this CompiledBindingExpression.BindingUpdateDelegate<T> func, DotvvmBindableObject contextControl, T value)
        {
            var dataContexts = GetDataContexts(contextControl);
            //var control = contextControl.GetClosestControlBindingTarget();
            func(dataContexts.ToArray(), contextControl, value);
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static object ExecDelegate(this CompiledBindingExpression.BindingDelegate func, DotvvmBindableObject contextControl)
        {
            var dataContexts = GetDataContexts(contextControl);
            return func(dataContexts.ToArray(), contextControl);
        }

        /// <summary>
        /// Prepares DataContext hierarchy argument and executes update delegate.
        /// </summary>
        public static T ExecDelegate<T>(this CompiledBindingExpression.BindingDelegate<T> func, DotvvmBindableObject contextControl)
        {
            var dataContexts = GetDataContexts(contextControl);
            return func(dataContexts.ToArray(), contextControl);
        }

        /// <summary>
        /// Gets all data context on the path to root. Maximum count can be specified by `count`
        /// </summary>
        public static IEnumerable<object> GetDataContexts(this DotvvmBindableObject contextControl, int count = -1)
        {
            var c = contextControl;
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
        public static object Evaluate(this IStaticValueBinding binding, DotvvmBindableObject control)
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
        /// Writes the value to binding - binded viewModel property is updated. May throw an exception when binding does not support assignment.
        /// </summary>
        public static void UpdateSource(this IUpdatableValueBinding binding, object value, DotvvmBindableObject control)
        {
            ExecUpdateDelegate(
                binding.UpdateDelegate,
                FindDataContextTarget(binding, control).target,
                value);
        }

        /// <summary>
        /// Writes the value to binding - binded viewModel property is updated. May throw an exception when binding does not support assignment.
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
                FindDataContextTarget(binding, control).target);
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
        public static object Evaluate(this ICommandBinding binding, DotvvmBindableObject control, params object[] args)
        {
            var action = binding.GetCommandDelegate(control);
            if (action is Command) return (action as Command)();
            if (action is Action) { (action as Action)(); return null; }
            return action.DynamicInvoke(args);
        }

        /// <summary>
        /// Gets DataContext-adjusted javascript that can be used for command invocation.
        /// </summary>
        public static ParametrizedCode GetParametrizedCommandJavascript(this ICommandBinding binding, DotvvmBindableObject control) =>
            JavascriptTranslator.AdjustKnockoutScriptContext(binding.CommandJavascript,
                dataContextLevel: FindDataContextTarget(binding, control).stepsUp);

        /// <summary>
        /// Creates new `TBinding` with the original DataContextStack, LocationInfo, AdditionalResolvers and BindingCompilationService. 
        /// </summary>
        public static TBinding DeriveBinding<TBinding>(this TBinding binding, DataContextStack newDataContext, Expression expression, params object[] properties)
            where TBinding : IBinding
        {
            return binding.DeriveBinding(
                properties.Concat(new object[]{
                    newDataContext,
                    new ParsedExpressionBindingProperty(expression)
                }).ToArray()
            );
        }

        /// <summary>
        /// Creates new `TBinding` with the original DataContextStack, LocationInfo, AdditionalResolvers and BindingCompilationService. 
        /// </summary>
        public static TBinding DeriveBinding<TBinding>(this TBinding binding, params object[] properties)
            where TBinding : IBinding
        {
            object[] getContextProperties(IBinding b) =>
                new object[] {
                    b.GetProperty<DataContextStack>(ErrorHandlingMode.ReturnNull),
                    b.GetProperty<BindingResolverCollection>(ErrorHandlingMode.ReturnNull),
                    b.GetProperty<BindingErrorReporterProperty>(ErrorHandlingMode.ReturnNull),
                    b.GetProperty<LocationInfoBindingProperty>(ErrorHandlingMode.ReturnNull)
                };
            var service = binding.GetProperty<BindingCompilationService>();
            return (TBinding)service.CreateBinding(binding.GetType(), getContextProperties(binding).Concat(properties).ToArray());
        }

        /// <summary>
        /// Caches all function evaluations in the closure based on parameter. TParam should be immutable, as it is used as Dictionary key.
        /// It thread-safe.
        /// </summary>
        public static Func<TParam, TResult> Cache<TParam, TResult>(this Func<TParam, TResult> func)
        {
            var cache = new ConcurrentDictionary<TParam, TResult>();
            return f => cache.GetOrAdd(f, func);
        }

        public static IValueBinding GetThisBinding(this DotvvmBindableObject obj)
        {
            var dataContext = obj.GetValueBinding(DotvvmBindableObject.DataContextProperty);
            return (IValueBinding)dataContext.GetProperty<ThisBindingProperty>().binding;
        }

        private static readonly ConditionalWeakTable<Expression, BindingParameterAnnotation> _expressionAnnotations =
            new ConditionalWeakTable<Expression, BindingParameterAnnotation>();
        public static TExpression AddParameterAnnotation<TExpression>(this TExpression expr, BindingParameterAnnotation annotation)
            where TExpression : Expression
        {
            _expressionAnnotations.Add(expr, annotation);
            return expr;
        }

        public static BindingParameterAnnotation GetParameterAnnotation(this Expression expr) =>
            _expressionAnnotations.TryGetValue(expr, out var annotation) ? annotation : null;

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
                else if (node.Name.StartsWith("_parent") && int.TryParse(node.Name.Substring("_parent".Length), out int index))
                    return node.AddParameterAnnotation(new BindingParameterAnnotation(DataContext.EnumerableItems().ElementAt(index)));
                return base.VisitParameter(node);
            }
        }

        public static CompiledBindingExpression.BindingDelegate<T> ToGeneric<T>(this CompiledBindingExpression.BindingDelegate d) => (a, b) => (T)d(a, b);
        public static CompiledBindingExpression.BindingUpdateDelegate<T> ToGeneric<T>(this CompiledBindingExpression.BindingUpdateDelegate d) => (a, b, c) => d(a, b, c);
    }


    public class BindingParameterAnnotation
    {
        public readonly DataContextStack DataContext;
        public readonly BindingExtensionParameter ExtensionParameter;

        public BindingParameterAnnotation(DataContextStack context = null, BindingExtensionParameter extensionParameter = null)
        {
            this.DataContext = context;
            this.ExtensionParameter = extensionParameter;
        }
    }
}
