using Microsoft.AspNetCore.Hosting;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmEnvironmentNameProvider : IEnvironmentNameProvider
    {
        private readonly IHostingEnvironment environment;

        public DotvvmEnvironmentNameProvider(IHostingEnvironment environment)
        {
            this.environment = environment;
        }

        public string GetEnvironmentName(IDotvvmRequestContext context)
            => environment.EnvironmentName;
    }
}