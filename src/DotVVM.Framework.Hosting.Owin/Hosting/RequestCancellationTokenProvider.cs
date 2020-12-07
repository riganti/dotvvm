using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.Owin.Hosting
{
    internal class RequestCancellationTokenProvider : IRequestCancellationTokenProvider
    {
        public CancellationToken GetCancellationToken(IDotvvmRequestContext context)
        {
            return context.GetOwinContext().Request.CallCancelled;
        }
    }
}
