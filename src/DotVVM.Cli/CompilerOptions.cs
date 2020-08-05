using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Cli
{
    public class CompilerOptions
    {
        //optional
        public string[]? DothtmlFiles { get; set; }

        /// <summary>
        /// Assembly that contains DotvvmStartup. Required.
        /// </summary>
        public string? WebSiteAssembly { get; set; }
        public bool OutputResolvedDothtmlMap { get; set; } = true;
        public string? BindingsAssemblyName { get; set; }
        public string? BindingClassName { get; set; }
        public string? OutputPath { get; set; }

        public string? AssemblyName { get; set; }

        /// <summary>
        /// Path to the parent directory of Views, Controls, etc.
        /// <summary>
        public string? WebSitePath { get; set; } 
        public bool FullCompile { get; set; } = false;
        public bool CheckBindingErrors { get; set; } = true;
        public bool SerializeConfig { get; set; }
        public string? ConfigOutputPath { get; set; }

        public static void ApplyDefaults(CompilerOptions options)
        {
            options.OutputPath ??= "./output";
            options.AssemblyName ??= "CompiledViews";
            options.BindingsAssemblyName ??= options.AssemblyName + "Bindings";
            options.BindingClassName ??= options.BindingsAssemblyName + "." + "CompiledBindings";
        }
    }
}
