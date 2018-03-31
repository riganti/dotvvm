using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Symbols;
using DotVVM.TypeScript.Compiler.Symbols.Filters;
using DotVVM.TypeScript.Compiler.Symbols.Registries;
using DotVVM.TypeScript.Compiler.Translators;
using DotVVM.TypeScript.Compiler.Translators.Symbols;
using DotVVM.TypeScript.Compiler.Utils;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler
{
    public class Compiler
    {
        private CompilerArguments compilerArguments;
        private readonly TypeRegistry typeRegistry;
        private readonly TranslatorsEvidence _translatorsEvidence;
        private readonly IFileStore _fileStore;
        private CompilerContext _compilerContext;

        public Compiler(CompilerArguments compilerArguments, IFileStore fileStore)
        {
            this.compilerArguments = compilerArguments;
            _fileStore = fileStore;
            this.typeRegistry = new TypeRegistry();
            this._translatorsEvidence = new TranslatorsEvidence();
        }


        public async Task RunAsync()
        {
            _compilerContext = await CreateCompilerContext();
            RegisterTranslators(_compilerContext);    
            FindTranslatableViewModels(_compilerContext);

            var translatedViewModels = TranslateViewModels();

            var typescriptViewModels = await StoreViewModels(translatedViewModels);
            var outputFilePath = CompileTypescript(typescriptViewModels);
        }

        private string CompileTypescript(IEnumerable<string> typescriptViewModels)
        {
            var basePath = FindProjectBasePath();
            var outputPath = Path.Combine(basePath, "dotvvm.viewmodels.generated.js");
            var arguments = $" {typescriptViewModels.StringJoin(" ")} --outfile {outputPath}";
            Process.Start(new ProcessStartInfo() {
                FileName = "tsc",
                Arguments = arguments,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            })?.WaitForExit();
            return string.Empty;
        }

        private async Task<IEnumerable<string>> StoreViewModels(List<TsSyntaxNode> translatedViewModels)
        {
            var filesList = new List<string>();
            var basePath = FindProjectBasePath();
            foreach (var viewModel in translatedViewModels)
            {
                if (viewModel is TsClassDeclarationSyntax @class)
                {
                    var filePath = Path.Combine(basePath, $"{@class.Identifier.Value}.ts");
                    await _fileStore.StoreFileAsync(filePath, viewModel.ToDisplayString());
                    filesList.Add(filePath);
                }
            }
            return filesList;
        }

        private string FindProjectBasePath()
        {
            var projectPath = FindProject(_compilerContext.Workspace).FilePath;
            var projectDirectory = new FileInfo(projectPath).Directory;
            var basePath = projectDirectory.FullName;
            if (projectDirectory.GetDirectories().Any(d => d.Name == "wwwroot"))
            {
                basePath = Path.Combine(basePath, "wwwroot");
            }
            basePath = Path.Combine(basePath, "Scripts");
            return basePath;
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
            return await FindProject(workspace)
                .GetCompilationAsync();
        }

        private Project FindProject(Workspace workspace)
        {
            return workspace.CurrentSolution
                .Projects
                .First(p => p.Name == compilerArguments.ProjectName);
        }

        private Workspace CreateWorkspace()
        {
            //Workaround before MsBuildWorkspace starts working on Linux
            var analyzerManager = new AnalyzerManager(compilerArguments.SolutionFile.ToString());
            return analyzerManager.GetWorkspace();
        }
    }
}
