using System;
using System.Linq;
using System.Text.RegularExpressions;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Binding
{
    public class ControlStateBindingExpression : ValueBindingExpression, IUpdatableBindingExpression
    {
        public ControlStateBindingExpression()
        {
        }

        public ControlStateBindingExpression(string expression)
            : base(expression)
        {
        }



        /// <summary>
        /// Evaluates the specified expression.
        /// </summary>
        public override object Evaluate(RedwoodBindableControl control, RedwoodProperty property)
        {
            ValidateExpression(Expression);

            // find the parent markup control and calculate number of DataContext changes
            int numberOfDataContextChanges;
            var current = RedwoodBindableControl.GetClosestControlBindingTarget(control, out numberOfDataContextChanges) as RedwoodBindableControl;

            if (current == null || !current.RequiresControlState)
            {
                throw new Exception("The {controlState: ...} binding can only be used in a markup control that supports ControlState!");    // TODO: exception handling
            }
            else
            {
                object value;
                return current.ControlState.TryGetValue(Expression, out value) ? value : null;
            }
        }

        /// <summary>
        /// Translates the expression to client script.
        /// </summary>
        public override string TranslateToClientScript(RedwoodBindableControl control, RedwoodProperty property)
        {
            ValidateExpression(Expression);

            // find the parent markup control and calculate number of DataContext changes
            int numberOfDataContextChanges;
            var current = RedwoodBindableControl.GetClosestControlBindingTarget(control, out numberOfDataContextChanges) as RedwoodBindableControl;

            current.EnsureControlHasId();
            return string.Join(".", Enumerable.Range(0, numberOfDataContextChanges).Select(i => "$parent").Concat(new[] { "$controlState()", current.ID + "()", Expression }));
        }

        /// <summary>
        /// Updates the value.
        /// </summary>
        public void UpdateSource(object value, RedwoodBindableControl control, RedwoodProperty property)
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