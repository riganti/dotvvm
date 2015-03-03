using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

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

            var trees = compilations.SelectMany(c => c.SyntaxTrees.Select(t => new SyntaxTreeInfo() { Tree = t, SemanticModel = c.GetSemanticModel(t), Compilation = c })).ToList();
            return trees;
        }
    }
}