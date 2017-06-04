using System.Collections.Generic;
using DotVVM.Framework.Configuration;

namespace DotVVM.Compiler.Light
{
    public class CompilationResult
    {
        public Dictionary<string, FileCompilationResult> Files { get; set; } = new Dictionary<string, FileCompilationResult>();
        public DotvvmConfiguration Configuration { get; set; }
    }
}