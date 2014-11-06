using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Classification
{
    /// <summary>
    /// A classifier that highlights bindings and directives in RwHtml file.
    /// </summary>
    public class RwHtmlClassifier : IClassifier
    {
        private readonly ITextBuffer buffer;
        private IClassificationType bindingBrace, bindingContent;
        private RwHtmlTokenizer tokenizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RwHtmlClassifier"/> class.
        /// </summary>
        public RwHtmlClassifier(IClassificationTypeRegistryService registry, ITextBuffer buffer)
        {
            tokenizer = new RwHtmlTokenizer();
            this.buffer = buffer;
            bindingBrace = registry.GetClassificationType(RwHtmlClassificationTypes.BindingBrace);
            bindingContent = registry.GetClassificationType(RwHtmlClassificationTypes.BindingContent);
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Gets all the <see cref="T:Microsoft.VisualStudio.Text.Classification.ClassificationSpan" /> objects that overlap the given range of text.
        /// </summary>
        /// <param name="span">The snapshot span.</param>
        /// <returns>
        /// A list of <see cref="T:Microsoft.VisualStudio.Text.Classification.ClassificationSpan" /> objects that intersect with the given range.
        /// </returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var spans = new List<ClassificationSpan>();
            Action<ClassificationSpan> addSpan = s =>
            {
                if (s.Span.IntersectsWith(span))
                {
                    spans.Add(s);
                }
            };

            // tokenize the text
            try
            {
                tokenizer.Tokenize(new StringReader(buffer.CurrentSnapshot.GetText()), "");   // TODO: get file name

                // return the spans
                var state = BindingState.OtherContent;
                var lastPosition = -1;
                var lastLine = -1;
                foreach (var token in tokenizer.Tokens)
                {
                    if (state == BindingState.OtherContent)
                    {
                        // start highlighting on binding brace or directive
                        if (token.Type == RwHtmlTokenType.OpenBinding || token.Type == RwHtmlTokenType.DirectiveStart)
                        {
                            addSpan(new ClassificationSpan(new SnapshotSpan(span.Snapshot, token.StartPosition, token.Length), bindingBrace));
                            lastPosition = token.StartPosition + token.Length;
                            lastLine = token.LineNumber;
                            state = token.Type == RwHtmlTokenType.OpenBinding ? BindingState.Binding : BindingState.Directive;
                        }
                    }
                    else if (state == BindingState.Binding)
                    {
                        // highlight binding content
                        if (token.Type == RwHtmlTokenType.OpenBinding || token.Type == RwHtmlTokenType.CloseBinding)
                        {
                            addSpan(new ClassificationSpan(new SnapshotSpan(span.Snapshot, lastPosition, token.StartPosition - lastPosition), bindingContent));
                            lastPosition = -1;
                            state = BindingState.OtherContent;
                        }

                        // highlight closing brace
                        if (token.Type == RwHtmlTokenType.CloseBinding)
                        {
                            addSpan(new ClassificationSpan(new SnapshotSpan(span.Snapshot, token.StartPosition, token.Length), bindingBrace));
                        }
                    }
                    else if (state == BindingState.Directive)
                    {
                        // highlight until end of the line
                        if (token.LineNumber > lastLine)
                        {
                            addSpan(new ClassificationSpan(new SnapshotSpan(span.Snapshot, lastPosition, token.StartPosition - lastPosition), bindingContent));
                            lastPosition = -1;
                            state = BindingState.OtherContent;
                        }   
                    }
                }
                
                // finish directive if it is at the end of the file
                if (state != BindingState.OtherContent)
                {
                    addSpan(new ClassificationSpan(new SnapshotSpan(span.Snapshot, lastPosition, span.Length - lastPosition), bindingContent));
                }
            }
            catch (Exception ex)
            {
            }

            return spans;
        }

    }

    public enum BindingState
    {
        OtherContent = 0,
        Binding = 1,
        Directive = 2
    }
}