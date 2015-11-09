using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpException : Exception
    {
        public DotvvmHttpException(string message)
            : base(message)
        {
        }

        public DotvvmHttpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}