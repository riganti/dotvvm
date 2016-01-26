using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Compiler
{
    public class CompilerOptions
    {
        public string[] DothtmlFiles { get; set; }
        public string WebSiteAssembly { get; set; }
        public bool OutputResolvedDothtmlMap { get; set; }
        public string BindingsAssemblyName { get; set; }
        public string BindingClassName { get; set; }
        public string OutputPath { get; set; }
        public string AssemblyName { get; set; }
        public string WebSitePath { get; set; }
        public bool FullCompile { get; set; } = true;
        public bool CheckBindingErrors { get; set; }
        public bool SerializeConfig { get; set; }
    }
}
