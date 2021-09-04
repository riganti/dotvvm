using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    internal sealed class DotvvmRequestContextStorage
    {
        public IDotvvmRequestContext Context;
    }
}