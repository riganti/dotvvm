using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE80;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Classification;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions
{
    public class RwHtmlCompletionSource : ICompletionSource
    {
        private readonly RwHtmlCompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        private readonly DTE2 dte;
        private readonly RwHtmlClassifier classifier;
        private readonly RwHtmlParser parser;


        public RwHtmlCompletionSource(RwHtmlCompletionSourceProvider sourceProvider, RwHtmlParser parser, 
            RwHtmlClassifier classifier, ITextBuffer textBuffer, DTE2 dte)
        {
            this.sourceProvider = sourceProvider;
            this.textBuffer = textBuffer;
            this.dte = dte;
            this.classifier = classifier;
            this.parser = parser;
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
                    IEnumerable<SimpleRwHtmlCompletion> items = null;
                    var context = new RwHtmlCompletionContext()
                    {
                        Tokens = classifier.Tokens,
                        CurrentTokenIndex = classifier.Tokens.IndexOf(currentToken),
                        Parser = parser,
                        Tokenizer = classifier.Tokenizer,
                        DTE = dte
                    };
                    parser.Parse(classifier.Tokens);
                    context.CurrentNode = parser.Root.FindNodeByPosition(session.TextView.Caret.Position.BufferPosition.Position);
                    
                    if (currentToken.Type == RwHtmlTokenType.DirectiveStart)
                    {
                        // directive name completion
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.DirectiveName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.WhiteSpace)
                    {
                        if (context.CurrentNode is RwHtmlDirectiveNode)
                        {
                            // directive value
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.DirectiveValue).SelectMany(p => p.GetItems(context));
                        }
                        else if (context.CurrentNode is RwHtmlElementNode)
                        {
                            // attribute name
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagAttributeName).SelectMany(p => p.GetItems(context));
                        }
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenTag)
                    {
                        // tag completion
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.SingleQuote || currentToken.Type == RwHtmlTokenType.DoubleQuote || currentToken.Type == RwHtmlTokenType.Equals)
                    {
                        // attribute
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagAttributeValue).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenBinding)
                    {
                        // binding name
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.BindingName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.Colon)
                    {
                        // binding value
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.BindingValue).SelectMany(p => p.GetItems(context));
                    }
                    else
                    {
                        items = Enumerable.Empty<SimpleRwHtmlCompletion>();
                    }

                    var results = items.ToList();
                    if (!results.Any())
                    {
                        session.Dismiss();
                    }
                    else
                    {
                        completionSets.Add(new CompletionSet("All", "All", FindTokenSpanAtPosition(session), results, null));
                    }
                }
            }
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session)
        {
            var currentPoint = session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot);
            return currentPoint.Snapshot.CreateTrackingSpan(currentPoint.Position, 0, SpanTrackingMode.EdgeInclusive);
        }


        public void Dispose()
        {
        }

    }
}
