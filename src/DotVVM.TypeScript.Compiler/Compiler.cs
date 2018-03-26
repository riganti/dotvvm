using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Symbols.Filters;
using DotVVM.TypeScript.Compiler.Symbols.Registries;
using DotVVM.TypeScript.Compiler.Translators;
using DotVVM.TypeScript.Compiler.Translators.Symbols;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler
{
    public class Compiler
    {
        private CompilerArguments compilerArguments;
        private readonly TypeRegistry typeRegistry;
        private readonly TranslatorsEvidence _translatorsEvidence;
        public Compiler(CompilerArguments compilerArguments)
        {
            this.compilerArguments = compilerArguments;
            this.typeRegistry = new TypeRegistry();
            this._translatorsEvidence = new TranslatorsEvidence();
        }


        public async Task RunAsync()
        {
            var compilerContext = await CreateCompilerContext();
            RegisterTranslators(compilerContext);    
            FindTranslatableViewModels(compilerContext);

            var translatedViewModels = TranslateViewModels();
            translatedViewModels
                .ForEach(t => Console.WriteLine(t.ToDisplayString()));
            translatedViewModels
                .ForEach();
        }

        private List<TsSyntaxNode> TranslateViewModels()
        {
            return typeRegistry.Types.Select(t => _translatorsEvidence.ResolveTranslator(t.Type).Translate(t.Type)).ToList();
        }

        private void FindTranslatableViewModels(CompilerContext compilerContext)
        {
            var visitor = new MultipleSymbolFinder(new ClientSideMethodFilter());
            var typesToTranslate = visitor
                .VisitAssembly(compilerContext.Compilation.Assembly)
                .GroupBy(m => m.ContainingType);
            foreach (var typeAndMethods in typesToTranslate)
            {
                typeRegistry.RegisterType(typeAndMethods.Key, typeAndMethods);
            }
        }

        public void RegisterTranslators(CompilerContext compilerContext)
        {
            _translatorsEvidence.RegisterTranslator(() => new MethodSymbolTranslator(_translatorsEvidence, compilerContext));
            _translatorsEvidence.RegisterTranslator(() => new PropertySymbolTranslator());
            _translatorsEvidence.RegisterTranslator(() => new ParameterSymbolTranslator());
            _translatorsEvidence.RegisterTranslator(() => new TypeSymbolTranslator(_translatorsEvidence));
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
