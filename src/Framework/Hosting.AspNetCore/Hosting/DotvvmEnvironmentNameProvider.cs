using Microsoft.AspNetCore.Hosting;

#if NET5_0_OR_GREATER
using HostingEnv = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
#else
using HostingEnv = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#endif

namespace DotVVM.Framework.Hosting
{
    public class DotvvmEnvironmentNameProvider : IEnvironmentNameProvider
    {
        private readonly HostingEnv environment;

        public DotvvmEnvironmentNameProvider(HostingEnv environment)
        {
            this.environment = environment;
        }

        public string GetEnvironmentName(IDotvvmRequestContext context)
            => environment.EnvironmentName;
    }
}
