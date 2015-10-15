using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
{
    public static class CompletionHelper
    {
        private static DTE2 dte = null;

        private static object dteLocker = new object();

        public static DTE2 DTE
        {
            get
            {
                if (dte == null)
                {
                    lock (dteLocker)
                    {
                        if (dte == null)
                        {
                            dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
                        }
                    }
                }
                return dte;
            }
        }

        public static bool IsWhiteSpaceTextToken(DothtmlToken token)
        {
            return token.Type == DothtmlTokenType.Text && String.IsNullOrWhiteSpace(token.Text);
        }

        public static List<SyntaxTreeInfo> GetSyntaxTrees(DothtmlCompletionContext context)
        {
            var compilations = GetCompilations(context);

            var trees = compilations
                .SelectMany(c => c.SyntaxTrees.Select(t => new SyntaxTreeInfo() { Tree = t, SemanticModel = c.GetSemanticModel(t), Compilation = c }))
                .Where(t => t.Tree != null)
                .ToList();
            return trees;
        }

        public static List<ITypeSymbol> GetReferencedSymbols(DothtmlCompletionContext context)
        {
            var compilations = GetCompilations(context);

            var symbols = compilations
                .SelectMany(c => c.References.Select(r => new { Compilation = c, Reference = r }))
                .SelectMany(r =>
                {
                    var symbol = r.Compilation.GetAssemblyOrModuleSymbol(r.Reference);
                    if (symbol is IModuleSymbol) return new[] { symbol }.OfType<IModuleSymbol>();
                    return ((IAssemblySymbol)symbol).Modules;
                })
                .SelectMany(m => GetAllTypesInModuleSymbol(m.GlobalNamespace))
                .ToList();

            return symbols;
        }

        public static IEnumerable<ProjectItem> GetCurrentProjectFiles(DothtmlCompletionContext context)
        {
            return DTEHelper.GetCurrentProjectItems();
        }

        public static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol type)
        {
            while (type.BaseType != null)
            {
                yield return type.BaseType;
                type = type.BaseType;
            }
        }

        public static bool IsAssignable(INamedTypeSymbol targetType, ITypeSymbol valueType)
        {
            return targetType.GetThisAndAllBaseTypes().Concat(targetType.AllInterfaces).Contains(valueType);
        }

        public static INamedTypeSymbol FindIEnumerableType(ITypeSymbol type)
        {
            return type.AllInterfaces.FirstOrDefault(i => i.IsGenericType && i.ContainingNamespace.ToDisplayString() == "System.Collections.Generic" && i.Name == "IEnumerable");
        }

        private static IEnumerable<ITypeSymbol> GetAllTypesInModuleSymbol(INamespaceSymbol symbol)
        {
            return Enumerable.Concat(symbol.GetTypeMembers(), symbol.GetNamespaceMembers().SelectMany(GetAllTypesInModuleSymbol));
        }

        private static List<Compilation> GetCompilations(DothtmlCompletionContext context)
        {
            var compilations = new List<Compilation>();

            foreach (var p in context.RoslynWorkspace.CurrentSolution.Projects)
            {
                try
                {
                    var compilation = Task.Run(() => p.GetCompilationAsync()).Result;
                    if (compilation != null)
                    {
                        compilations.Add(compilation);
                    }
                }
                catch (Exception ex)
                {
                    LogService.LogError(new Exception("Cannot get the compilation!", ex));
                }
            }

            return compilations;
        }
    }
}