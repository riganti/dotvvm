using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Parser;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(RwHtmlCompletionProviderBase))]
    public class TypeNameDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {

        private CachedValue<List<string>> typeNames = new CachedValue<List<string>>(); 

        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, string directiveName)
        {
            if (directiveName == Constants.ViewModelDirectiveName || directiveName == Constants.BaseTypeDirective)
            {
                var types = typeNames.GetOrRetrieve(() =>
                {
                    return CompletionHelper.GetSyntaxTrees(context)
                        .SelectMany(i => i.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                        .Select(n => new { Symbol = i.SemanticModel.GetDeclaredSymbol(n), Compilation = i.Compilation })
                        .Where(n => n.Symbol != null))
                        .Select(t => t.Symbol.ToString() + ", " + t.Compilation.AssemblyName)
                        .ToList();
                });

                return types.Select(t => new SimpleRwHtmlCompletion(t));
            }
            else
            {
                return Enumerable.Empty<SimpleRwHtmlCompletion>();
            }
        }

        protected override void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs workspaceChangeEventArgs)
        {
            typeNames.ClearCachedValue();
        }
    }
}
