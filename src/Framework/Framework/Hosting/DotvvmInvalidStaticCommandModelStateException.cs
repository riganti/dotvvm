using System;
using System.Diagnostics;

namespace DotVVM.Framework.Hosting
{
    [DebuggerStepThrough]
    internal class DotvvmInvalidStaticCommandModelStateException : Exception
    {
        public readonly StaticCommandModelState StaticCommandModelState;

        [DebuggerHidden]
        public DotvvmInvalidStaticCommandModelStateException(StaticCommandModelState staticCommandModelState)
        {
            StaticCommandModelState = staticCommandModelState;
        }

        [DebuggerHidden]
        public DotvvmInvalidStaticCommandModelStateException(StaticCommandModelState staticCommandModelState, string message, Exception innerException)
            : base(message, innerException)
        {
            StaticCommandModelState = staticCommandModelState;
        }
    }
}
