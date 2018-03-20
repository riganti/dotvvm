using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler
{
    public struct CompilerArguments
    {
        public FileInfo SolutionFile  { get; set; }
        public string ProjectName { get; set; }
    }

    public class Compiler
    {
        private CompilerArguments compilerArguments;

        public Compiler(CompilerArguments compilerArguments)
        {
            this.compilerArguments = compilerArguments;
        }

        
        public async Task RunAsync()
        {
            var workspace = CreateWorkspace();
            var compilation = await workspace.CurrentSolution.Projects.First(p => p.Name == compilerArguments.ProjectName).GetCompilationAsync();

        }

        private Workspace CreateWorkspace()
        {
            var analyzerManager = new AnalyzerManager(compilerArguments.SolutionFile.ToString());
            return analyzerManager.GetWorkspace();
        }
    }
}
