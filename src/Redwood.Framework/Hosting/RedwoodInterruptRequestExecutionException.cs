using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Hosting
{
    /// <summary>
    /// An exception which is used to interrupt the request processing pipeline because the response has already been generated.
    /// </summary>
    public class RedwoodInterruptRequestExecutionException : ApplicationException
    {

        public RedwoodInterruptRequestExecutionException()
        {
        }

        public RedwoodInterruptRequestExecutionException(string message) : base(message)
        {
        }

        public RedwoodInterruptRequestExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}