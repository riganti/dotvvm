using DotVVM.Framework.Configuration;

namespace DotVVM.CompilationStatusPage
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Status", "", "Views/Status.dothtml");
        }
    }
}
