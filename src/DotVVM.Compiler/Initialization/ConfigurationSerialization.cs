using DotVVM.Compiler.Fakes;
using DotVVM.Compiler.Programs;

namespace DotVVM.Compiler.Initialization
{
    internal class ConfigurationSerialization
    {
        public static void PreInit()
        {
            var converter = new OfflineResourceRepositoryJsonConverter();
            converter.GetResourceTypeNames();
        }
    }
}