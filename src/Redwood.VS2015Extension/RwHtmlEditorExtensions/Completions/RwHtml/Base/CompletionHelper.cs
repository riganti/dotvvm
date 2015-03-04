using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Project = EnvDTE.Project;

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
            var compilations = context.RoslynWorkspace.CurrentSolution.Projects
                .Select(p =>
                {
                    Compilation compilation;
                    return p.TryGetCompilation(out compilation) ? compilation : null;
                })
                .Where(p => p != null)
                .ToList();

            var trees = compilations
                .SelectMany(c => c.SyntaxTrees.Select(t => new SyntaxTreeInfo() { Tree = t, SemanticModel = c.GetSemanticModel(t), Compilation = c }))
                .Where(t => t.Tree != null)
                .ToList();
            return trees;
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
                foreach (var childItem in GetSelfAndChildProjectItems(projectItem.ProjectItems.Item(i)))
                {
                    yield return childItem;
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
    }
}