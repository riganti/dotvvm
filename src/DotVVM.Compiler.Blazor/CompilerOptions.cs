using System.IO;
using System.Linq;
using DotVVM.Framework.Configuration;

namespace DotVVM.Compiler.Blazor
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
        public string WebSitePath { get; set; } = ".";
        public bool FullCompile { get; set; } = true;
        public bool CheckBindingErrors { get; set; }
        public bool SerializeConfig { get; set; }

        public void InitMissingOptions()
        {
            if (this.OutputPath == null) this.OutputPath = "./output";
            if (this.AssemblyName == null) this.AssemblyName = "CompiledViews";
            if (this.BindingsAssemblyName == null) this.BindingsAssemblyName = this.AssemblyName + "Bindings";
            if (this.BindingClassName == null) this.BindingClassName = this.BindingsAssemblyName + "." + "CompiledBindings";

            OutputPath = Path.GetFullPath(OutputPath);
            WebSiteAssembly = Path.GetFullPath(WebSiteAssembly);
            WebSitePath = Path.GetFullPath(WebSitePath);
        }

        public void PopulateRouteTable(DotvvmConfiguration config)
        {
            if (this.DothtmlFiles == null)
            {
                this.DothtmlFiles = config.RouteTable.Select(r => r.VirtualPath).ToArray();
            }
        }
    }
}