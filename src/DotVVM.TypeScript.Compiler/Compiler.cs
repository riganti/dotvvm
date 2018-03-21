using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Symbols.Filters;
using DotVVM.TypeScript.Compiler.Symbols.Registries;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler
{
    public class Compiler
    {
        private CompilerArguments compilerArguments;
        private readonly TypeRegistry typeRegistry;
        public Compiler(CompilerArguments compilerArguments)
        {
            this.compilerArguments = compilerArguments;
            this.typeRegistry = new TypeRegistry();
        }


        public async Task RunAsync()
        {
            var compilerContext = await CreateCompilerContext();
             
            var visitor = new MultipleSymbolFinder(new ClientSideMethodFilter());
            var typesToTranslate = visitor
                    .VisitAssembly(compilerContext.Compilation.Assembly)
                    .GroupBy(m => m.ContainingType);
            foreach (var typeAndMembers in typesToTranslate)
            {
                typeRegistry.RegisterType(typeAndMembers.Key, typeAndMembers);
            }
        }

        private async Task<CompilerContext> CreateCompilerContext()
        {
            var workspace = CreateWorkspace();
            var compilation = await CompileProject(workspace);
            var compilerContext = new CompilerContext { Compilation = compilation, Workspace = workspace };
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
