using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Project = EnvDTE.Project;
using System.Threading.Tasks;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public static class CompletionHelper
    {
        public static bool IsWhiteSpaceTextToken(RwHtmlToken token)
        {
            return token.Type == RwHtmlTokenType.Text && String.IsNullOrWhiteSpace(token.Text);
        }

        public static List<SyntaxTreeInfo> GetSyntaxTrees(RwHtmlCompletionContext context)
        {
            var compilations = GetCompilations(context);

            var trees = compilations
                .SelectMany(c => c.SyntaxTrees.Select(t => new SyntaxTreeInfo() { Tree = t, SemanticModel = c.GetSemanticModel(t), Compilation = c }))
                .Where(t => t.Tree != null)
                .ToList();
            return trees;
        }

        public static List<ITypeSymbol> GetReferencedSymbols(RwHtmlCompletionContext context)
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

        private static IEnumerable<ITypeSymbol> GetAllTypesInModuleSymbol(INamespaceSymbol symbol)
        {
            return Enumerable.Concat(symbol.GetTypeMembers(), symbol.GetNamespaceMembers().SelectMany(GetAllTypesInModuleSymbol));
        }

        private static List<Compilation> GetCompilations(RwHtmlCompletionContext context)
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
                    // TODO: report compilation error
                }
            }

            return compilations;
        }

        public static IEnumerable<ProjectItem> GetCurrentProjectFiles(RwHtmlCompletionContext context)
        {
            return context.DTE.ActiveDocument.ProjectItem.ContainingProject.ProjectItems.OfType<ProjectItem>().SelectMany(GetSelfAndChildProjectItems);
        }

        private static IEnumerable<ProjectItem> GetSelfAndChildProjectItems(ProjectItem projectItem)
        {
            yield return projectItem;
            for (int i = 1; i <= projectItem.ProjectItems.Count; i++)
            {
                ProjectItem item = null;
                try
                {
                    item = projectItem.ProjectItems.Item(i);
                }
                catch (Exception)
                {
                    // sometimes we get System.ArgumentException: The parameter is incorrect. (Exception from HRESULT: 0x80070057 (E_INVALIDARG)) 
                    // when we open some file in the text editor
                }

                if (item != null)
                {
                    foreach (var childItem in GetSelfAndChildProjectItems(item))
                    {
                        yield return childItem;
                    }
                }
            }
        }

        public static string GetProjectItemRelativePath(ProjectItem item)
        {
            var path = item.Properties.Item("FullPath").Value as string;
            var projectPath = GetProjectPath(item.ContainingProject);

            var result = path.StartsWith(projectPath, StringComparison.CurrentCultureIgnoreCase) ? path.Substring(projectPath.Length).TrimStart('\\', '/') : path;
            result = result.Replace('\\', '/');
            return result;
        }

        public static string GetProjectPath(Project project)
        {
            return project.Properties.Item("FullPath").Value as string;
        }

        public static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol type)
        {
            while (type.BaseType != null)
            {
                yield return type.BaseType;
                type = type.BaseType;
            }
        }
    }
}