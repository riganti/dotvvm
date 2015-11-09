using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Exceptions
{
    public static class ExceptionUtils
    {
        public static void ForInnerExceptions<TException>(this Exception ex, Action<TException> action)
            where TException: Exception
        {
            if (ex is TException) action((TException)ex);

            if(ex is AggregateException)
            {
                foreach (var inex in (ex as AggregateException).InnerExceptions)
                {
                    inex.ForInnerExceptions(action);
                }
            }
            else if (ex.InnerException != null) ex.InnerException.ForInnerExceptions(action);
        }
    }
}
