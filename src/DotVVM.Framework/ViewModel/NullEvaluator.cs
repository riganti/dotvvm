using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.ViewModel
{
    public class NullEvaluator : IExpressionEvaluator<object>
    {
        public object Evaluate(string expression)
        {
            return null;
        }
    }
}