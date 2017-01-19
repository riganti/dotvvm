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

namespace DotVVM.Framework.Binding
{
    public static class BindingHelper
    {
        public static T GetProperty<T>(this IBinding binding, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException) => (T)binding.GetProperty(typeof(T), errorMode);

        public static string GetKnockoutBindingExpression(this IValueBinding binding) =>
            JavascriptTranslator.FormatKnockoutScript(binding.KnockoutExpression);

        public static string GetKnockoutBindingExpression(this IValueBinding binding, DotvvmBindableObject currentControl) =>
            JavascriptTranslator.FormatKnockoutScript(binding.KnockoutExpression,
                dataContextLevel: FindDataContextTarget(binding, currentControl).stepsUp);

        public static ParametrizedCode GetParametrizedKnockoutExpression(this IValueBinding binding, DotvvmBindableObject currentControl) =>
            JavascriptTranslator.AdjustKnockoutScriptContext(binding.KnockoutExpression, dataContextLevel: FindDataContextTarget(binding, currentControl).stepsUp);

        // PERF: maybe safe last GetValue's target/binding to ThreadLocal variable, so the path does not have to be traversed twice
        public static (int stepsUp, DotvvmBindableObject target) FindDataContextTarget(this IBinding binding, DotvvmBindableObject control)
        {
            var controliId = (int)control.GetValue(Internal.DataContextSpaceIdProperty);
            var bindingId = binding.GetProperty<DataContextSpaceIdBindingProperty>(ErrorHandlingMode.ReturnNull)?.Id;
            if (bindingId == null || controliId == -1 || controliId == bindingId) return (0, control);

            var changes = 0;
            foreach (var a in control.GetAllAncestors())
            {
                if (a.properties.ContainsKey(DotvvmBindableObject.DataContextProperty)) changes++;
                if (a.GetValue(Internal.DataContextSpaceIdProperty, inherit: false) as int? == bindingId)
                    return (changes, a);
            }
            throw new NotSupportedException($"Could not find DataContextSpace of binding '{binding}'.");
        }

        public static void ExecUpdateDelegate(this CompiledBindingExpression.BindingUpdateDelegate func, DotvvmBindableObject contextControl, object value, bool seeThis)
        {
            var dataContexts = GetDataContexts(contextControl, seeThis);
            var control = contextControl.GetClosestControlBindingTarget();
            func(dataContexts.ToArray(), control, value);
        }

        public static object ExecDelegate(this CompiledBindingExpression.BindingDelegate func, DotvvmBindableObject contextControl, bool seeThis)
        {
            var dataContexts = GetDataContexts(contextControl, seeThis);
            var control = contextControl.GetClosestControlBindingTarget();
            try
            {
                return func(dataContexts.ToArray(), control);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all data context on the path to root
        /// </summary>
        public static IEnumerable<object> GetDataContexts(this DotvvmBindableObject contextControl, bool seeThis, bool crossMarkupControl = false)
        {
            if (!seeThis) contextControl = contextControl?.Parent;

            var c = contextControl;
            while(c != null)
            {
                // PERF: O(h^2) because GetValue calls another GetDataContexts
                if (c.IsPropertySet(DotvvmBindableObject.DataContextProperty, inherit: false))
                    yield return c.GetValue(DotvvmBindableObject.DataContextProperty);

                if (c is DotvvmMarkupControl && !crossMarkupControl) yield break;

                c = c.Parent;
            }
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public static object Evaluate(this IStaticValueBinding binding, DotvvmBindableObject control)
        {
            return ExecDelegate(
                binding.BindingDelegate,
                FindDataContextTarget(binding, control).target,
                binding.GetProperty<IncludesThisDataContextBindingFlag>(ErrorHandlingMode.ReturnNull) == null);
        }

        /// <summary>
        /// Updates the viewModel with the new value.
        /// </summary>
        public static void UpdateSource(this IUpdatableValueBinding binding, object value, DotvvmBindableObject control)
        {
            ExecUpdateDelegate(
                binding.UpdateDelegate,
                FindDataContextTarget(binding, control).target,
                value,
                binding.GetProperty<IncludesThisDataContextBindingFlag>(ErrorHandlingMode.ReturnNull) == null);
        }

        public static Delegate GetCommandDelegate(this ICommandBinding binding, DotvvmBindableObject control)
        {
            return (Delegate)ExecDelegate(
                binding.BindingDelegate,
                FindDataContextTarget(binding, control).target,
                binding.GetProperty<IncludesThisDataContextBindingFlag>(ErrorHandlingMode.ReturnNull) == null);
        }

        public static object Evaluate(this ICommandBinding binding, DotvvmBindableObject control, params object[] args)
        {
            var action = binding.GetCommandDelegate(control);
            if (action is Command) return (action as Command)();
            if (action is Action) { (action as Action)(); return null; }
            return action.DynamicInvoke(args);
        }

        public static ParametrizedCode GetParametrizedCommandJavascript(this ICommandBinding binding, DotvvmBindableObject control) =>
            JavascriptTranslator.AdjustKnockoutScriptContext(binding.CommandJavascript,
                dataContextLevel: FindDataContextTarget(binding, control).stepsUp);

//        public static string GetCommandJavascript(this ICommandBinding binding, DotvvmBindableObject control, 
//            CodeParameterAssignment viewModelName,
//			CodeParameterAssignment SenderElementParameter,
//			CodeParameterAssignment CurrentPathParameter,
//			CodeParameterAssignment commandId,
//			CodeParameterAssignment controlUniqueId,
//			CodeParameterAssignment 
//            ) =>
//            binding.GetParametrizedCommandJavascript(control).ToString(o =>
//                
//
//            );
    }
}
