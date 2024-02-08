using System;

namespace DotVVM.Framework.Testing
{
    class ViewCompilationFakeRequestContext : TestDotvvmRequestContext
    {
        public ViewCompilationFakeRequestContext(IServiceProvider services): base(services)
        {
        }
    }
}
