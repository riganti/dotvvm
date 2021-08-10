#nullable enable
using System;

namespace DotVVM.Framework.Routing
{
    public class DotvvmRouteStrategyException : DotvvmRouteException
    {

        public DotvvmRouteStrategyException(string message)
            : base(message)
        {

        }

        public DotvvmRouteStrategyException(string message, Exception? innerException)
            : base(message, innerException)
        {

        }
    }
}
