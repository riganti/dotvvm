using System;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser.Translation;

namespace Redwood.Framework.Binding
{
    /// <summary>
    /// A binding that gets the value from a viewmodel property.
    /// </summary>
    public class ValueBindingExpression : BindingExpression
    {

        private static ExpressionTranslator translator = new ExpressionTranslator();
        private static ExpressionEvaluator evaluator = new ExpressionEvaluator();


        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBindingExpression"/> class.
        /// </summary>
        public ValueBindingExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBindingExpression"/> class.
        /// </summary>
        public ValueBindingExpression(string expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public override object Evaluate(RedwoodBindableControl control, RedwoodProperty property)
        {
            object parentValue;
            if (property == RedwoodBindableControl.DataContextProperty)
            {
                // DataContext evaluates in the parent's DataContext
                parentValue = control.Parent.GetValue(RedwoodBindableControl.DataContextProperty);
            }
            else
            {
                // other properties evaluate in the current DataContext
                parentValue = control.GetValue(RedwoodBindableControl.DataContextProperty);
            }

            return evaluator.Evaluate(Expression, parentValue);
        }

        /// <summary>
        /// Translates the binding to client script.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="property"></param>
        public override string TranslateToClientScript(RedwoodBindableControl control, RedwoodProperty property)
        {
            return translator.Translate(Expression);
        }
    }
}