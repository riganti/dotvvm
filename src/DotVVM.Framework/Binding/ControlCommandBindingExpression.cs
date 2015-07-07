using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Binding
{
    public class ControlCommandBindingExpression : CommandBindingExpression
    {

        
        public ControlCommandBindingExpression()
        {
        }

        public ControlCommandBindingExpression(string expression)
            : base(expression)
        {
        }

    }
}