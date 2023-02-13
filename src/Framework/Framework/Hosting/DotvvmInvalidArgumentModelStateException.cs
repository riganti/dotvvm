using System;
using System.Diagnostics;

namespace DotVVM.Framework.Hosting
{
    [DebuggerStepThrough]
    internal class DotvvmInvalidArgumentModelStateException : Exception
    {
        public readonly ArgumentModelState ArgumentModelState;

        [DebuggerHidden]
        public DotvvmInvalidArgumentModelStateException(ArgumentModelState argumentModelState)
        {
            ArgumentModelState = argumentModelState;
        }

        [DebuggerHidden]
        public DotvvmInvalidArgumentModelStateException(ArgumentModelState argumentModelState, string message, Exception innerException)
            : base(message, innerException)
        {
            ArgumentModelState = argumentModelState;
        }
    }
}
