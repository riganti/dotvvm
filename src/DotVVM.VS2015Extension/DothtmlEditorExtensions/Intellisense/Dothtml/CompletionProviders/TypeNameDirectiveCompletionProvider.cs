using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions.CustomCommit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.CompletionProviders
{
    [Export(typeof(IDothtmlCompletionProvider))]
    public class TypeNameDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {
        private CachedValue<List<CompletionDataWithGlyph>> typeNames = new CachedValue<List<CompletionDataWithGlyph>>();

        public override void OnWorkspaceChanged()
        {
            typeNames.ClearCachedValue();
        }

        protected override IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, string directiveName)
        {
            if (string.Equals(directiveName, ParserConstants.ViewModelDirectiveName, StringComparison.InvariantCultureIgnoreCase) 
                || string.Equals(directiveName, ParserConstants.BaseTypeDirective, StringComparison.InvariantCultureIgnoreCase))
            {
                // get icons for intellisense
                var classGlyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
                var interfaceGlyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupInterface, StandardGlyphItem.GlyphItemPublic);
                var nameFilter = string.Empty;

                var currentToken = context.Tokens[context.CurrentTokenIndex];
                if (currentToken.Type == DothtmlTokenType.DirectiveValue)
                {
                    var currentPosition = context.CompletionSession.TextView.Caret.Position.BufferPosition.Position;
                    if (currentPosition != currentToken.StartPosition)
                    {
                        nameFilter = string.Concat(currentToken.Text.Take(currentPosition - currentToken.StartPosition));
                    }
                }

                // get list of all custom types
                var types = typeNames.GetOrRetrieve(() =>
                {
                    return CompletionHelper.GetSyntaxTrees(context)
                        .SelectMany(i => i.Tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                        .Select(n => new { Symbol = i.SemanticModel.GetDeclaredSymbol(n), Compilation = i.Compilation, Node = n })
                        .Where(n => n.Symbol != null))
                        .Select(t => new CompletionDataWithGlyph()
                        {
                            CompletionData = new CompletionData(
                                                                $"{t.Symbol.Name} (in namespace {t.Symbol.ContainingNamespace})",
                                                                t.Symbol.ToString() + ", " + t.Compilation.AssemblyName),
                            Glyph = t.Node is ClassDeclarationSyntax ? classGlyph : interfaceGlyph,
                            Name = t.Symbol.Name,
                            Namespace = t.Symbol.ContainingNamespace.ToString()
                        })
                        .ToList();
                });

                if (!string.IsNullOrWhiteSpace(nameFilter))
                {
                    types = types.Where(w =>
                    w.Name.StartsWith(nameFilter, StringComparison.OrdinalIgnoreCase)
                    || ($"{w.Namespace}.{w.Name}").StartsWith(nameFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // return completion items
                return types.Select(t => new[]
                {
                  new SimpleDothtmlCompletion(t.CompletionData.DisplayText, t.CompletionData.CompletionText, t.Glyph),
                  new SimpleDothtmlCompletion(t.CompletionData.CompletionText, t.CompletionData.CompletionText, t.Glyph)
                })
                .SelectMany(sm => sm);
            }
            else
            {
                return Enumerable.Empty<SimpleDothtmlCompletion>();
            }
        }

        internal class CompletionDataWithGlyph
        {
            public CompletionData CompletionData { get; set; }
            public ImageSource Glyph { get; set; }
            public string Name { get; set; }
            public string Namespace { get; set; }
        }
    }
}