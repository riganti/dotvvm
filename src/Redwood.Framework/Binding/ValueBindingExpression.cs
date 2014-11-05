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
            if (property != RedwoodBindableControl.DataContextProperty)
            {
                throw new Exception("Server evaluation of properties other than DataContext, is not allowed!");   // TODO: exception handling
            }

            var parentValue = control.Parent.GetValue(RedwoodBindableControl.DataContextProperty);
            return evaluator.Evaluate(Expression, parentValue);
        }

        /// <summary>
        /// Translates the binding to client script.
        /// </summary>
        public override string TranslateToClientScript()
        {
            return translator.Translate(Expression);
        }
    }
}