using DotVVM.Compiler.Fakes;
using DotVVM.Compiler.Programs;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;

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
