#nullable enable
using System;

namespace DotVVM.Framework.ResourceManagement
{
    internal class DotvvmResourceException : Exception
    {
        public DotvvmResourceException()
        {
        }

        public DotvvmResourceException(string message) : base(message)
        {
        }

        public DotvvmResourceException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    internal class DotvvmLinkResourceException : DotvvmResourceException
    {
        public DotvvmLinkResourceException(string message) : base(message)
        {
        }
    }
}
