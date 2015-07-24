using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Translation;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
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
        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            return evaluator.Evaluate(this, property, control);
        }

        /// <summary>
        /// Translates the binding to client script.
        /// </summary>
        public override string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property)
        {
            return translator.Translate(Expression);
        }

        /// <summary>
        /// Gets the view model path expression.
        /// </summary>
        public virtual string GetViewModelPathExpression(DotvvmBindableControl control, DotvvmProperty property)
        {
            return Expression;
        }

        /// <summary>
        /// Updates the viewModel with the new value.
        /// </summary>
        public virtual void UpdateSource(object value, DotvvmBindableControl control, DotvvmProperty property)
        {
            object target;
            var propertyInfo = evaluator.EvaluateProperty(this, property, control, out target);
            propertyInfo.SetValue(target, ReflectionUtils.ConvertValue(value, propertyInfo.PropertyType));
        }



        /// <summary>
        /// Creates an expression for specified member property. 
        /// </summary>
        public static ValueBindingExpression FromMember(string propertyName)
        {
            return new ValueBindingExpression(string.IsNullOrEmpty(propertyName) ? Constants.ThisSpecialBindingProperty : propertyName);
        }
    }
}