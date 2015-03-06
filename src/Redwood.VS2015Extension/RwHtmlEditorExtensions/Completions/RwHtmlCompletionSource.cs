using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Classification;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    public class RwHtmlCompletionSource : ICompletionSource
    {
        private readonly RwHtmlCompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        private readonly VisualStudioWorkspace workspace;
        private readonly IGlyphService glyphService;
        private readonly DTE2 dte;
        private readonly RedwoodConfigurationProvider configurationProvider;
        private readonly RwHtmlClassifier classifier;
        private readonly RwHtmlParser parser;
        private readonly MetadataControlResolver metadataControlResolver;


        public RwHtmlCompletionSource(RwHtmlCompletionSourceProvider sourceProvider, RwHtmlParser parser, 
            RwHtmlClassifier classifier, ITextBuffer textBuffer, VisualStudioWorkspace workspace, 
            IGlyphService glyphService, DTE2 dte, RedwoodConfigurationProvider configurationProvider,
            MetadataControlResolver metadataControlResolver)
        {
            this.sourceProvider = sourceProvider;
            this.textBuffer = textBuffer;
            this.classifier = classifier;
            this.parser = parser;
            this.workspace = workspace;
            this.glyphService = glyphService;
            this.dte = dte;
            this.configurationProvider = configurationProvider;
            this.metadataControlResolver = metadataControlResolver;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var tokens = classifier.Tokens;
            if (tokens != null)
            {
                // find current token
                var cursorPosition = session.TextView.Caret.Position.BufferPosition;
                var currentToken = tokens.FirstOrDefault(t => t.StartPosition <= cursorPosition && t.StartPosition + t.Length >= cursorPosition);
                if (currentToken != null) 
                {
                    IEnumerable<SimpleRwHtmlCompletion> items = Enumerable.Empty<SimpleRwHtmlCompletion>();
                    var context = new RwHtmlCompletionContext()
                    {
                        Tokens = classifier.Tokens,
                        CurrentTokenIndex = classifier.Tokens.IndexOf(currentToken),
                        Parser = parser,
                        Tokenizer = classifier.Tokenizer,
                        RoslynWorkspace = workspace,
                        GlyphService = glyphService,
                        DTE = dte,
                        Configuration = configurationProvider.GetConfiguration(dte.ActiveDocument.ProjectItem.ContainingProject),
                        MetadataControlResolver = metadataControlResolver
                    };
                    parser.Parse(classifier.Tokens);
                    context.CurrentNode = parser.Root.FindNodeByPosition(session.TextView.Caret.Position.BufferPosition.Position - 1);

                    var combineWithHtmlCompletions = false;

                    if (currentToken.Type == RwHtmlTokenType.DirectiveStart)
                    {
                        // directive name completion
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.DirectiveName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.WhiteSpace)
                    {
                        if (context.CurrentNode is RwHtmlDirectiveNode && context.CurrentTokenIndex >= 2 && tokens[context.CurrentTokenIndex - 2].Type == RwHtmlTokenType.DirectiveStart)
                        {
                            // directive value
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.DirectiveValue).SelectMany(p => p.GetItems(context));
                        }
                        else if (context.CurrentNode is RwHtmlElementNode)
                        {
                            // attribute name
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagAttributeName).SelectMany(p => p.GetItems(context));
                            combineWithHtmlCompletions = true;
                        }
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenTag)
                    {
                        // element name
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagName).SelectMany(p => p.GetItems(context));
                        combineWithHtmlCompletions = true;
                    }
                    else if (currentToken.Type == RwHtmlTokenType.SingleQuote || currentToken.Type == RwHtmlTokenType.DoubleQuote || currentToken.Type == RwHtmlTokenType.Equals)
                    {
                        // attribute value
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagAttributeValue).SelectMany(p => p.GetItems(context));
                        combineWithHtmlCompletions = true;
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenBinding)
                    {
                        // binding name
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.BindingName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.Colon)
                    {
                        if (context.CurrentNode is RwHtmlBindingNode)
                        {
                            // binding value
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.BindingValue).SelectMany(p => p.GetItems(context));
                        }
                        else
                        {
                            // element name
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagName).SelectMany(p => p.GetItems(context));
                            combineWithHtmlCompletions = true;
                        }
                    }

                    var results = items.OrderBy(v => v.DisplayText).ToList();
                    if (!results.Any())
                    {
                        session.Dismiss();
                    }
                    else
                    {
                        var newCompletionSet = new CustomCompletionSet("HTML", "HTML", FindTokenSpanAtPosition(session), results, null);
                        if (combineWithHtmlCompletions && completionSets.Any())
                        {
                            newCompletionSet = MergeCompletionSets(completionSets, newCompletionSet);
                        }
                        else
                        {
                            completionSets.Clear();
                        }
                        completionSets.Add(newCompletionSet);
                    }
                }
            }
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            var currentPoint = session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot);
            return currentPoint.Snapshot.CreateTrackingSpan(currentPoint.Position, 0, SpanTrackingMode.EdgeInclusive);
        }

        private static CustomCompletionSet MergeCompletionSets(IList<CompletionSet> completionSets, CustomCompletionSet newCompletions)
        {
            var htmlCompletionsSet = completionSets.First();
            
            var mergedCompletionSet = new CustomCompletionSet(
                htmlCompletionsSet.Moniker,
                htmlCompletionsSet.DisplayName,
                htmlCompletionsSet.ApplicableTo,
                newCompletions.Completions.Concat(htmlCompletionsSet.Completions).OrderBy(n => n.DisplayText),
                htmlCompletionsSet.CompletionBuilders);

            completionSets.Remove(htmlCompletionsSet);

            return mergedCompletionSet;
        }

        public void Dispose()
        {
        }

    }
}
