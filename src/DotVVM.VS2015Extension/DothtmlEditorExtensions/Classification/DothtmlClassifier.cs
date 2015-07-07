using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    /// <summary>
    /// A classifier that highlights bindings and directives in Dothtml file.
    /// </summary>
    public class DothtmlClassifier : IClassifier
    {
        private readonly ITextBuffer buffer;
        private IClassificationType bindingBrace, bindingContent;
        private DothtmlTokenizer tokenizer;

        public IList<DothtmlToken> Tokens
        {
            get { return tokenizer.Tokens; }
        }

        public DothtmlTokenizer Tokenizer
        {
            get { return tokenizer; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DothtmlClassifier"/> class.
        /// </summary>
        public DothtmlClassifier(IClassificationTypeRegistryService registry, ITextBuffer buffer)
        {
            tokenizer = new DothtmlTokenizer();
            this.buffer = buffer;
            bindingBrace = registry.GetClassificationType(DothtmlClassificationTypes.BindingBrace);
            bindingContent = registry.GetClassificationType(DothtmlClassificationTypes.BindingContent);
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
                tokenizer.Tokenize(new StringReader(buffer.CurrentSnapshot.GetText()));

                // return the spans
                var state = BindingState.OtherContent;
                var lastPosition = -1;
                var lastLine = -1;
                foreach (var token in tokenizer.Tokens)
                {
                    if (state == BindingState.OtherContent)
                    {
                        // start highlighting on binding brace or directive
                        if (token.Type == DothtmlTokenType.OpenBinding || token.Type == DothtmlTokenType.DirectiveStart)
                        {
                            addSpan(new ClassificationSpan(new SnapshotSpan(span.Snapshot, token.StartPosition, token.Length), bindingBrace));
                            lastPosition = token.StartPosition + token.Length;
                            lastLine = token.LineNumber;
                            state = token.Type == DothtmlTokenType.OpenBinding ? BindingState.Binding : BindingState.Directive;
                        }
                    }
                    else if (state == BindingState.Binding)
                    {
                        // highlight binding content
                        if (token.Type == DothtmlTokenType.OpenBinding || token.Type == DothtmlTokenType.CloseBinding)
                        {
                            addSpan(new ClassificationSpan(new SnapshotSpan(span.Snapshot, lastPosition, token.StartPosition - lastPosition), bindingContent));
                            lastPosition = -1;
                            state = BindingState.OtherContent;
                        }

                        // highlight closing brace
                        if (token.Type == DothtmlTokenType.CloseBinding)
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
                LogService.LogError(new Exception("Classifier error!", ex));
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