using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Language.Intellisense;
using Redwood.Framework.Parser;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    [Export(typeof(IRwHtmlCompletionProvider))]
    public class TypeNameDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {

        private CachedValue<List<CompletionData>> typeNames = new CachedValue<List<CompletionData>>(); 


        protected override IEnumerable<SimpleRwHtmlCompletion> GetItemsCore(RwHtmlCompletionContext context, string directiveName)
        {
            if (directiveName == Constants.ViewModelDirectiveName || directiveName == Constants.BaseTypeDirective)
            {
                // get list of all custom types
                var types = typeNames.GetOrRetrieve(() =>
                {
                    return CompletionHelper.GetSyntaxTrees(context)
                        .SelectMany(i => i.Tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
                        .Select(n => new { Symbol = i.SemanticModel.GetDeclaredSymbol(n), Compilation = i.Compilation })
                        .Where(n => n.Symbol != null))
                        .Select(t => new CompletionData(
                            string.Format("{0} (in namespace {1})", t.Symbol.Name, t.Symbol.ContainingNamespace),
                            t.Symbol.ToString() + ", " + t.Compilation.AssemblyName))
                        .ToList();
                });

                // return completion items
                var glyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
                return types.Select(t => new SimpleRwHtmlCompletion(t.DisplayText, t.CompletionText, glyph));
            }
            else
            {
                return Enumerable.Empty<SimpleRwHtmlCompletion>();
            }
        }

        public override void OnWorkspaceChanged()
        {
            typeNames.ClearCachedValue();
        }
    }
}
