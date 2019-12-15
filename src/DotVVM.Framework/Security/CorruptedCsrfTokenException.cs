using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace DotVVM.Framework.Security
{
    public class CorruptedCsrfTokenException: Exception
    {
        /// If the client is supposed to retry the request after renewing the CSRF token by the <see cref="Middlewares.DotvvmCsrfTokenMiddleware" />
        public bool RetryRequest { get; }
        protected CorruptedCsrfTokenException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public CorruptedCsrfTokenException(string message, bool retry = true) : base(message)
        {
            this.RetryRequest = retry;
        }

        public CorruptedCsrfTokenException(string message, Exception inner, bool retry = true) : base(message, inner)
        {
            this.RetryRequest = retry;
        }
    }
}
