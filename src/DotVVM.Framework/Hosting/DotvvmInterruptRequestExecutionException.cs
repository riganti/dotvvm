using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// An exception which is used to interrupt the request processing pipeline because the response has already been generated.
    /// </summary>
    public class DotvvmInterruptRequestExecutionException : ApplicationException
    {

        public InterruptReason InterruptReason { get; set; }

        public string CustomData { get; set; }

        public DotvvmInterruptRequestExecutionException()
        {
        }

        public DotvvmInterruptRequestExecutionException(InterruptReason interruptReason, string customData = null)
        {
            InterruptReason = interruptReason;
            CustomData = customData;
        }
        
        public DotvvmInterruptRequestExecutionException(string message) : base(message)
        {
        }

        public DotvvmInterruptRequestExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}