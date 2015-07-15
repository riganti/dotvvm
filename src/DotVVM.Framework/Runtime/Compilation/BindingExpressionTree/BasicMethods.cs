using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public static class BasicMethods
    {
        public static MethodInvocation CreateIIF(Type type, BindingExpressionNode condition, BindingExpressionNode ifTrue, BindingExpressionNode ifFalse)
        {
            var method = typeof(BasicMethods).GetMethod("IIF").MakeGenericMethod(type);
            return new MethodInvocation(null, method, condition, ifTrue, ifFalse);
        }

        public static T IIF<T>(bool condition, T ifTrue, T ifFalse)
        {
            return condition ? ifTrue : ifFalse;
        }

        public static bool Not(bool wh) => !wh;
    }
}
