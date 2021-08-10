using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting
{
    public interface IRequestCancellationTokenProvider
    {
        CancellationToken GetCancellationToken(IDotvvmRequestContext context);
    }
}
