using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.ViewModel
{
    public class NullEvaluator : IExpressionEvaluator<object>
    {
        public object Evaluate(string expression)
        {
            return null;
        }
    }
}