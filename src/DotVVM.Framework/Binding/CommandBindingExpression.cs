using System;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser.Translation;

namespace DotVVM.Framework.Binding
{
    public class CommandBindingExpression : BindingExpression
    {

        private static ExpressionTranslator translator = new ExpressionTranslator();


        public CommandBindingExpression()
        {
        }

        public CommandBindingExpression(string expression)
            : base(expression)
        {
        }


        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            // TODO: implement server evaluation
            throw new NotImplementedException();
        }

        /// <summary>
        /// Translates the binding to client script.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="property"></param>
        public override string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property)
        {
            return translator.Translate(Expression);
        }

        
    }
}