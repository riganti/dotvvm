using System.Runtime;

namespace DotVVM.Utils.ProjectService.Operations.Providers
{
    public class DotvvmToolMetadata
    {
        public DotvvmToolExecutableVersion Version { get; set; }
        public string MainModulePath { get; set; }
        public string TempDirectory { get; set; }

    }
}
