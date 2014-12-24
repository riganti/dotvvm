using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Binding
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