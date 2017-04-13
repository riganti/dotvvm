using DotVVM.Framework.Configuration;

namespace swag
{
    public class DotvvmStartup : IDotvvmStartup
    {
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Generator", "", "Views/Generator.dothtml");
        }
    }
}
