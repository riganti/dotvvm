using System;
using System.Linq;
using System.Text.RegularExpressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class ControlStateBindingExpression : ValueBindingExpression
    {
        public ControlStateBindingExpression()
        {
        }

        public ControlStateBindingExpression(string expression)
            : base(expression)
        {
        }


        /// <summary>
        /// Gets or sets the default value of the binding.
        /// </summary>
        public object DefaultValue { get; set; }



        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            ValidateExpression(Expression);

            // find the parent markup control and calculate number of DataContext changes
            int numberOfDataContextChanges;
            var current = control.GetClosestControlBindingTarget(out numberOfDataContextChanges) as DotvvmBindableControl;

            if (current == null || !current.RequiresControlState)
            {
                throw new Exception("The {controlState: ...} binding can only be used in a markup control that supports ControlState!");    // TODO: exception handling
            }
            else
            {
                object value;
                return current.ControlState.TryGetValue(Expression, out value) ? value : DefaultValue;
            }
        }

        /// <summary>
        /// Translates the expression to client script.
        /// </summary>
        public override string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property)
        {
            ValidateExpression(Expression);

            // find the parent markup control and calculate number of DataContext changes
            int numberOfDataContextChanges;
            var current = control.GetClosestControlBindingTarget(out numberOfDataContextChanges) as DotvvmBindableControl;

            current.EnsureControlHasId();
            return string.Join(".", Enumerable.Range(0, numberOfDataContextChanges).Select(i => "$parent").Concat(new[] { "$controlState()", current.ID + "()", Expression }));
        }

        /// <summary>
        /// Gets the view model path expression.
        /// </summary>
        public override string GetViewModelPathExpression(DotvvmBindableControl control, DotvvmProperty property)
        {
            // find the parent markup control and calculate number of DataContext changes
            int numberOfDataContextChanges;
            var current = control.GetClosestControlBindingTarget(out numberOfDataContextChanges) as DotvvmBindableControl;

            current.EnsureControlHasId();
            return string.Join(".", Enumerable.Range(0, numberOfDataContextChanges).Select(i => "_parent").Concat(new[] { "_controlState_" + current.ID, Expression }));
        }

        /// <summary>
        /// Updates the value.
        /// </summary>
        public override void UpdateSource(object value, DotvvmBindableControl control, DotvvmProperty property)
        {
            control.ControlState[property.Name] = value;
        }

        /// <summary>
        /// Validates the expression.
        /// </summary>
        public static void ValidateExpression(string expression)
        {
            if (!Regex.IsMatch(expression, "^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                throw new Exception("The {controlState: ...} binding can only contain a property name!");       // TODO: exception handling
            }
        }

    }
}