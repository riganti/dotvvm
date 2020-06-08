//
// THIS FILE IS LINKED TO DOTVVM CLI TOOL
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Utils.ProjectService.Lookup;

namespace DotVVM.Compiler
{
    public class CompilerOptions
    {
        //optional
        public string[] DothtmlFiles { get; set; }

        //required
        public string WebSiteAssembly { get; set; }
        public bool OutputResolvedDothtmlMap { get; set; } = true;
        public string BindingsAssemblyName { get; set; }
        public string BindingClassName { get; set; }
        public string OutputPath { get; set; }

        public string AssemblyName { get; set; }
        public TargetFramework TargetFramework { get; set; }
        public string CompilationConfiguration { get; set; }
        public string WebSitePath { get; set; } 
        public bool FullCompile { get; set; } = false;
        public bool CheckBindingErrors { get; set; } = true;
        public bool SerializeConfig { get; set; }
        public string ConfigOutputPath { get; set; }
    }
}
