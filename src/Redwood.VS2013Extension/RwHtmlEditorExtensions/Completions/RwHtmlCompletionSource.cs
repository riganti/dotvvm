using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Classification;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions
{
    public class RwHtmlCompletionSource : ICompletionSource
    {
        private readonly RwHtmlCompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        private RwHtmlClassifier classifier;

        [ImportMany(typeof(IRwHtmlCompletionProvider))]
        public IRwHtmlCompletionProvider[] CompletionProviders { get; set; }

        public RwHtmlCompletionSource(RwHtmlCompletionSourceProvider sourceProvider, RwHtmlClassifier classifier, ITextBuffer textBuffer)
        {
            this.sourceProvider = sourceProvider;
            this.textBuffer = textBuffer;
            this.classifier = classifier;
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
                        Parser = null,
                        Tokenizer = classifier.Tokenizer
                    };

                    if (currentToken.Type == RwHtmlTokenType.DirectiveStart)
                    {
                        // directive name completion
                        items = CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.DirectiveName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.Text && string.IsNullOrWhiteSpace(currentToken.Text) && context.CurrentTokenIndex >= 3)
                    {
                        if (CompletionHelper.IsWhiteSpaceTextToken(context.Tokens[context.CurrentTokenIndex - 1])
                            && context.Tokens[context.CurrentTokenIndex - 2].Type == RwHtmlTokenType.Text)
                        {
                            if (context.Tokens[context.CurrentTokenIndex - 3].Type == RwHtmlTokenType.DirectiveStart)
                            {
                                // directive value
                                items = CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.DirectiveName).SelectMany(p => p.GetItems(context));
                            }
                            else
                            {
                                // attribute name
                                // TODO: we need parser here
                            }
                        }
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenTag)
                    {
                        // tag completion
                        items = CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.SingleQuote || currentToken.Type == RwHtmlTokenType.DoubleQuote)
                    {
                        // attribute
                        items = CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.TagAttributeValue).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenBinding)
                    {
                        // binding name
                        items = CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.BindingName).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == RwHtmlTokenType.Colon)
                    {
                        // binding value
                        items = CompletionProviders.Where(p => p.TriggerPoint == TriggerPoint.BindingValue).SelectMany(p => p.GetItems(context));
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
