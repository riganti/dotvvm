using DotVVM.Utils.ProjectService.Lookup;

namespace DotVVM.Utils.ProjectService
{
    public class DotvvmProjectSertviceConfiguration
    {
        public bool Help { get; set; }
        public bool Build { get; set; }
        public bool Restore { get; set; }
        public string MsBuildPath { get; set; } = "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Enterprise\\MSBuild\\15.0\\Bin\\msbuild.exe";
        public bool DotvvmCompiler => !string.IsNullOrWhiteSpace(DotvvmCompilerNetPath) || !string.IsNullOrWhiteSpace(DotvvmCompilerCorePath);
        public string DotvvmCompilerNetPath { get; set; }
        public string DotvvmCompilerCorePath { get; set; }
        public string LookupFolder { get; set; }
        public string Filter { get; set; }
        public CsprojVersion Version { get; set; }
        public string StatisticsFolder { get; set; }
    }
}
