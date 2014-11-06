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

                    completionSets.Add(new CompletionSet("All", "All",
                        FindTokenSpanAtPosition(session.GetTriggerPoint(textBuffer), session),
                        items, null));
                }
            }
        }
        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = sourceProvider.NavigatorService.GetTextStructureNavigator(textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }


        public void Dispose()
        {
        }

        private IEnumerable<SimpleRwHtmlCompletion> GetDirectiveHints()
        {
            yield return new SimpleRwHtmlCompletion("viewmodel", "@viewmodel: ");
        }

        private IEnumerable<SimpleRwHtmlCompletion> GetTagNameHints()
        {
            yield return new SimpleRwHtmlCompletion("tag1");
            yield return new SimpleRwHtmlCompletion("tag2");
            yield return new SimpleRwHtmlCompletion("tag3");
        }

        private IEnumerable<SimpleRwHtmlCompletion> GetAttributeNameHints()
        {
            yield return new SimpleRwHtmlCompletion("attr1");
            yield return new SimpleRwHtmlCompletion("attr2");
            yield return new SimpleRwHtmlCompletion("attr3");
        }

        private IEnumerable<SimpleRwHtmlCompletion> GetBindingTypeHints()
        {
            yield return new SimpleRwHtmlCompletion("value", "value: ");
            yield return new SimpleRwHtmlCompletion("command", "command: ");
        }
    }
}
