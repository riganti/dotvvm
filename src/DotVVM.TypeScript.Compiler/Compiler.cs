using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Symbols.Filters;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler
{
    public struct CompilerArguments
    {
        public FileInfo SolutionFile { get; set; }
        public string ProjectName { get; set; }
    }

    internal struct CompilerContext
    {
        public Workspace Workspace { get; set; }
        public Compilation Compilation { get; set; }
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
            var compilerContext = await CreateCompilerContext();
            var visitor = new MultipleSymbolFinder(new ClientSideMethodFilter());
            var methodsToTranslate = visitor.VisitAssembly(compilerContext.Compilation.Assembly);
        }

        private async Task<CompilerContext> CreateCompilerContext()
        {
            var workspace = CreateWorkspace();
            var compilation = await CompileProject(workspace);
            var compilerContext = new CompilerContext {Compilation = compilation, Workspace = workspace};
            return compilerContext;
        }

        private async Task<Compilation> CompileProject(Workspace workspace)
        {
            return await workspace.CurrentSolution
                .Projects
                .First(p => p.Name == compilerArguments.ProjectName)
                .GetCompilationAsync();
        }

        private Workspace CreateWorkspace()
        {
            //Workaround before MsBuildWorkspace starts working on Linux
            var analyzerManager = new AnalyzerManager(compilerArguments.SolutionFile.ToString());
            return analyzerManager.GetWorkspace();
        }
    }
}
