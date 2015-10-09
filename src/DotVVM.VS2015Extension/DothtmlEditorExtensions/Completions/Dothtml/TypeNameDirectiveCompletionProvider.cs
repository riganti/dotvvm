using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Language.Intellisense;
using DotVVM.Framework.Parser;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    [Export(typeof(IDothtmlCompletionProvider))]
    public class TypeNameDirectiveCompletionProvider : DirectiveValueHtmlCompletionProviderBase
    {

        private CachedValue<List<CompletionDataWithGlyph>> typeNames = new CachedValue<List<CompletionDataWithGlyph>>(); 


        protected override IEnumerable<SimpleDothtmlCompletion> GetItemsCore(DothtmlCompletionContext context, string directiveName)
        {
            if (directiveName == Constants.ViewModelDirectiveName || directiveName == Constants.BaseTypeDirective)
            {
                // get icons for intellisense
                var classGlyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupClass, StandardGlyphItem.GlyphItemPublic);
                var interfaceGlyph = context.GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupInterface, StandardGlyphItem.GlyphItemPublic);
             
                // get list of all custom types
                var types = typeNames.GetOrRetrieve(() =>
                {
                    return CompletionHelper.GetSyntaxTrees(context)
                        .SelectMany(i => i.Tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                        .Select(n => new { Symbol = i.SemanticModel.GetDeclaredSymbol(n), Compilation = i.Compilation , Node = n})
                        .Where(n => n.Symbol != null))
                        .Select(t => new CompletionDataWithGlyph(){
                                            CompletionData  =  new CompletionData(
                                                                $"{t.Symbol.Name} (in namespace {t.Symbol.ContainingNamespace})",
                                                                t.Symbol.ToString() + ", " + t.Compilation.AssemblyName),
                                            Glyph = t.Node is ClassDeclarationSyntax ? classGlyph: interfaceGlyph
                                })
                        .ToList();
                });

                // return completion items
                return types.Select(t => new SimpleDothtmlCompletion(t.CompletionData .DisplayText, t.CompletionData.CompletionText, t.Glyph));
            }
            else
            {
                return Enumerable.Empty<SimpleDothtmlCompletion>();
            }
        }

        public override void OnWorkspaceChanged()
        {
            typeNames.ClearCachedValue();
        }

        internal class CompletionDataWithGlyph 
        {

            public CompletionData CompletionData { get; set; }
            public ImageSource Glyph { get; set; }
            
        }
    }
}
