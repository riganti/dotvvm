#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// An exception which is used to interrupt the request processing pipeline because the response has already been generated.
    /// </summary>
    [DebuggerStepThrough]
    public class DotvvmInterruptRequestExecutionException : Exception
    {
        public InterruptReason InterruptReason { get; set; }

        public string? CustomData { get; set; }

        [DebuggerHidden]
        public DotvvmInterruptRequestExecutionException()
        {
        }

        [DebuggerHidden]
        public DotvvmInterruptRequestExecutionException(InterruptReason interruptReason, string? customData = null)
            : base($"Request interrupted: {interruptReason} ({customData})")
        {
            InterruptReason = interruptReason;
            CustomData = customData;
        }

        [DebuggerHidden]
        public DotvvmInterruptRequestExecutionException(string message) : base(message)
        {
        }

        [DebuggerHidden]
        public DotvvmInterruptRequestExecutionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
