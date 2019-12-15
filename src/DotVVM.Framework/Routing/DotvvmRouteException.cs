#nullable enable
using System;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRouteException : Exception
    {

        public DotvvmRouteException(string message)
            : base(message)
        {

        }

        public DotvvmRouteException(string message, Exception? innerException)
            : base(message, innerException)
        {

        }
    }
}
