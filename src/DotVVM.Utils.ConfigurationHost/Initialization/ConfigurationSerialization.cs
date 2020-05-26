using DotVVM.Compiler.Fakes;

namespace DotVVM.Utils.ConfigurationHost.Initialization
{
    public class ConfigurationSerialization
    {
        public static void PreInit()
        {
            var converter = new OfflineResourceRepositoryJsonConverter();
            //TODO: verify whether this init is needed or not
            //converter.GetResourceTypeNames();
        }
    }
}
