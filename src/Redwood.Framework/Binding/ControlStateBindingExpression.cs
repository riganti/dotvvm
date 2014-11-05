using System;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Binding
{
    public class ControlStateBindingExpression : BindingExpression
    {
        public ControlStateBindingExpression()
        {
        }

        public ControlStateBindingExpression(string expression)
            : base(expression)
        {
        }

        public override object Evaluate(RedwoodBindableControl control, RedwoodProperty property)
        {
            throw new NotImplementedException();
        }

        public override string TranslateToClientScript()
        {
            throw new NotImplementedException();
        }
    }
}