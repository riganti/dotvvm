using System;

namespace DotVVM.Framework.Hosting
{
    internal class DotvvmFileStorageMissingException : Exception
    {
        public DotvvmFileStorageMissingException()
        {
        }

        public DotvvmFileStorageMissingException(string message) : base(message)
        {
        }

        public DotvvmFileStorageMissingException(string message, Exception? innerException) : base(message, innerException)
        {
        }
    }

}
