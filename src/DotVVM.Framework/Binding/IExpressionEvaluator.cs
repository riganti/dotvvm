using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Binding
{
    public interface IExpressionEvaluator<T>
    {

        T Evaluate(string expression);

    }
}
