using DotVVM.Compiler.Fakes;

namespace DotVVM.Utils.ConfigurationHost.Initialization
{
    public class ConfigurationSerialization
    {
        public static void PreInit()
        {
            var converter = new OfflineResourceRepositoryJsonConverter();
            converter.GetResourceTypeNames();
        }
    }
}
