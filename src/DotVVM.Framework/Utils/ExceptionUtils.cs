using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Utils
{
    public static class ExceptionUtils
    {
        public static void ForInnerExceptions<TException>(this Exception ex, Action<TException> action)
            where TException : Exception
        {
            if (ex is TException) action((TException)ex);

            if (ex is AggregateException aggregate)
            {
                foreach (var inex in aggregate.InnerExceptions)
                {
                    inex.ForInnerExceptions(action);
                }
            }
            else if (ex.InnerException != null) ex.InnerException.ForInnerExceptions(action);
        }

        public static IEnumerable<Exception> AllInnerExceptions(this Exception ex)
        {
            var stack = new Stack<Exception>(new[] { ex });
            while (stack.Any())
            {
                ex = stack.Pop();
                yield return ex;
                if (ex is AggregateException aggregate)
                {
                    foreach (var inex in aggregate.InnerExceptions)
                    {
                        stack.Push(inex);
                    }
                }
                else if (ex.InnerException != null)
                    stack.Push(ex.InnerException);
            }
        }
    }
}
