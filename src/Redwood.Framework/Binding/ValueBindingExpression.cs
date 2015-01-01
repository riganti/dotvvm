using System;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser.Translation;
using Redwood.Framework.Utils;

namespace Redwood.Framework.Binding
{
    /// <summary>
    /// A binding that gets the value from a viewmodel property.
    /// </summary>
    public class ValueBindingExpression : BindingExpression, IUpdatableBindingExpression
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
            return evaluator.Evaluate(this, property, control);
        }

        /// <summary>
        /// Translates the binding to client script.
        /// </summary>
        public override string TranslateToClientScript(RedwoodBindableControl control, RedwoodProperty property)
        {
            return translator.Translate(Expression);
        }

        /// <summary>
        /// Gets the view model path expression.
        /// </summary>
        public virtual string GetViewModelPathExpression(RedwoodBindableControl control, RedwoodProperty property)
        {
            return Expression;
        }

        /// <summary>
        /// Updates the viewModel with the new value.
        /// </summary>
        public virtual void UpdateSource(object value, RedwoodBindableControl control, RedwoodProperty property)
        {
            object target;
            var propertyInfo = evaluator.EvaluateProperty(this, property, control, out target);
            propertyInfo.SetValue(target, ReflectionUtils.ConvertValue(value, propertyInfo.PropertyType));
        }
    }
}