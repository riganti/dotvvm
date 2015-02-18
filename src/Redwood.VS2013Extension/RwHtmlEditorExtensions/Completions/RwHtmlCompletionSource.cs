using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.VS2013Extension.RwHtmlEditorExtensions.Classification;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions
{
    public class RwHtmlCompletionSource : ICompletionSource
    {
        private readonly RwHtmlCompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        private RwHtmlClassifier classifier;


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
                if (currentToken != null) {
                    IEnumerable<SimpleRwHtmlCompletion> items;
                    if (currentToken.Type == RwHtmlTokenType.DirectiveStart)
                    {
                        // directive completion
                        items = GetDirectiveHints();
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenTag)
                    {
                        // tag completion
                        items = GetTagNameHints();
                    }
                    else if (currentToken.Type == RwHtmlTokenType.SingleQuote || currentToken.Type == RwHtmlTokenType.DoubleQuote)
                    {
                        // attribute
                        items = GetAttributeNameHints();
                    }
                    else if (currentToken.Type == RwHtmlTokenType.OpenBinding)
                    {
                        // binding
                        items = GetBindingTypeHints();
                    }
                    else
                    {
                        items = Enumerable.Empty<SimpleRwHtmlCompletion>();
                    }

                    completionSets.Add(new CompletionSet("All", "All", FindTokenSpanAtPosition(session), items.ToList(), null));
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

        private IEnumerable<SimpleRwHtmlCompletion> GetDirectiveHints()
        {
            yield return new SimpleRwHtmlCompletion("viewmodel", "viewmodel ");
        }

        private IEnumerable<SimpleRwHtmlCompletion> GetTagNameHints()
        {
            yield return new SimpleRwHtmlCompletion("tag1", "tag1");
            yield return new SimpleRwHtmlCompletion("tag2", "tag2");
            yield return new SimpleRwHtmlCompletion("tag3", "tag3");
        }

        private IEnumerable<SimpleRwHtmlCompletion> GetAttributeNameHints()
        {
            yield return new SimpleRwHtmlCompletion("attr1", "attr1=\"");
            yield return new SimpleRwHtmlCompletion("attr2", "attr1=\"");
            yield return new SimpleRwHtmlCompletion("attr3", "attr1=\"");
        }

        private IEnumerable<SimpleRwHtmlCompletion> GetBindingTypeHints()
        {
            yield return new SimpleRwHtmlCompletion("value", "value: ");
            yield return new SimpleRwHtmlCompletion("command", "command: ");
        }
    }
}
