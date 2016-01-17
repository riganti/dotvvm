using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.VS2015Extension.Configuration;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Projection;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using Microsoft.VisualStudio.JSLS;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    public enum BindingState
    {
        OtherContent = 0,
        Binding = 1,
        Directive = 2
    }

    /// <summary>
    /// A classifier that highlights bindings and directives in Dothtml file.
    /// </summary>
    public class DothtmlClassifier : IClassifier
    {
        private static readonly Type _jsTaggerType = typeof(JavaScriptLanguageService).Assembly.GetType("Microsoft.VisualStudio.JSLS.Classification.Tagger");
        private readonly ITextBuffer buffer;
        private IClassificationType bindingBrace, bindingContent;
        private DothtmlTokenizer tokenizer;
        private IClassifierProvider[] otherProviders;
        private ITaggerProvider[] taggerProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="DothtmlClassifier"/> class.
        /// </summary>
        public DothtmlClassifier(IClassificationTypeRegistryService registry, ITextBuffer buffer, IClassifierProvider[] allClassifierProviders, ITaggerProvider[] taggerProviders)
        {
            tokenizer = new DothtmlTokenizer();
            this.buffer = buffer;
            bindingBrace = registry.GetClassificationType(DothtmlClassificationTypes.BindingBrace);
            bindingContent = registry.GetClassificationType(DothtmlClassificationTypes.BindingContent);
            otherProviders = allClassifierProviders
                .WhereContentTypeAttributeNot(ContentTypeDefinitions.DothtmlContentType)
                .ToArray();
            this.taggerProviders = taggerProviders;
        }

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged
        {
            add { }
            remove { }
        }

        public List<DothtmlToken> Tokens
        {
            get { return tokenizer.Tokens; }
        }

        public DothtmlTokenizer Tokenizer
        {
            get { return tokenizer; }
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

            var projectionInfos = DothtmlProjectionTextViewModelProvider.GetProjectionInfos(buffer.CurrentSnapshot);
            foreach (var projectionInfo in projectionInfos)
            {
                foreach (var provider in otherProviders.WhereContentTypeAttribute(ContentTypeDefinitions.JavaScriptContentType))
                {
                    if (projectionInfo.ContentType == ContentTypeDefinitions.JavaScriptContentType)
                    {
                        foreach (var tagger in taggerProviders.WhereContentTypeAttribute(ContentTypeDefinitions.JavaScriptContentType)
                                                    .Select(s => s.CreateTagger<ClassificationTag>(buffer)).Where(w => w != null))
                        {
                            buffer.SetProperty(tagger.GetType(), tagger);
                        }
                    }
                    var classifier = provider.GetClassifier(buffer);
                    if (classifier != null)
                    {
                        var otherSpans = classifier.GetClassificationSpans(new SnapshotSpan(buffer.CurrentSnapshot, projectionInfo.Start, projectionInfo.End - projectionInfo.Start));
                        spans.AddRange(otherSpans);
                    }
                }
            }

            // tokenize the text
            try
            {
                tokenizer.Tokenize(new StringReader(buffer.CurrentSnapshot.GetText()));

                // return the spans
                var state = BindingState.OtherContent;
                var lastPosition = -1;
                var lastLine = -1;
                for (int tokenIndex = 0; tokenIndex < tokenizer.Tokens.Count; tokenIndex++)
                {
                    var token = tokenizer.Tokens[tokenIndex];
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

            return spans.Where(w => !projectionInfos.Any(any => w.Span.Contains(any.Start))).ToList();
        }
    }
}