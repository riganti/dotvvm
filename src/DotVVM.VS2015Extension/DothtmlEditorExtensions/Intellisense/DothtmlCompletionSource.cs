using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.VS2015Extension.Configuration;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.CompletionProviders;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using EnvDTE80;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using Debugger = System.Diagnostics.Debugger;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense
{
    public class DothtmlCompletionSource : ICompletionSource
    {
        internal static List<ICompletionSession> activeSessions = new List<ICompletionSession>();
        private readonly DothtmlCompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        private readonly VisualStudioWorkspace workspace;
        private readonly IGlyphService glyphService;
        private readonly DTE2 dte;
        private readonly DotvvmConfigurationProvider configurationProvider;
        private readonly DothtmlClassifier classifier;
        private readonly DothtmlParser parser;
        private readonly ICompletionSourceProvider[] completionProviders;
        private readonly ITaggerProvider[] taggerProviders;
        private readonly IProjectionBufferFactoryService projectionBufferFactoryService;

        public DothtmlCompletionSource(DothtmlCompletionSourceProvider sourceProvider, DothtmlParser parser,
            DothtmlClassifier classifier, ITextBuffer textBuffer, VisualStudioWorkspace workspace,
            IGlyphService glyphService, DTE2 dte, DotvvmConfigurationProvider configurationProvider,
            MetadataControlResolver metadataControlResolver, ICompletionSourceProvider[] completionProviders,
            IProjectionBufferFactoryService projectionBufferFactoryService, ITaggerProvider[] taggerProviders)
        {
            this.sourceProvider = sourceProvider;
            this.textBuffer = textBuffer;
            this.classifier = classifier;
            this.parser = parser;
            this.workspace = workspace;
            this.glyphService = glyphService;
            this.dte = dte;
            this.configurationProvider = configurationProvider;
            this.MetadataControlResolver = metadataControlResolver;
            this.completionProviders = completionProviders;
            this.projectionBufferFactoryService = projectionBufferFactoryService;
            this.taggerProviders = taggerProviders;
        }

        public MetadataControlResolver MetadataControlResolver { get; private set; }

        public DothtmlCompletionContext GetCompletionContext(ICompletionSession session)
        {
            var context = new DothtmlCompletionContext()
            {
                CompletionSession = session,
                Tokens = classifier.Tokens,
                Parser = parser,
                Tokenizer = classifier.Tokenizer,
                RoslynWorkspace = workspace,
                GlyphService = glyphService,
                DTE = dte,
                Configuration = configurationProvider.GetConfiguration(dte.ActiveDocument.ProjectItem.ContainingProject),
                MetadataControlResolver = MetadataControlResolver
            };

            try
            {
                parser.Parse(classifier.Tokens);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw;
            }

            return context;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            var completionSetsCount = completionSets.Count;
            var tokens = classifier.Tokens;
            if (tokens != null)
            {
                // find current token
                var cursorPosition = session.TextView.Caret.Position.BufferPosition;

                var currentTokenIndex = FindCurrentTokenIndex(tokens, cursorPosition.Position);
                if (currentTokenIndex >= 0)
                {
                    // prepare the context
                    var currentToken = classifier.Tokens[currentTokenIndex];
                    var items = Enumerable.Empty<SimpleDothtmlCompletion>();
                    var context = GetCompletionContext(session);
                    context.CompletionSession = session;
                    context.CurrentTokenIndex = currentTokenIndex;
                    context.CurrentNode = parser.Root.FindNodeByPosition(cursorPosition.Position - 1);
                    var combineWithHtmlCompletions = false;
                    int? applicableToStartPosition = null;

                    TriggerPoint triggerPoint = TriggerPoint.None;
                    if (currentToken.Type == DothtmlTokenType.DirectiveStart)
                    {
                        // directive name completion
                        triggerPoint = TriggerPoint.DirectiveName;
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == DothtmlTokenType.DirectiveValue)
                    {
                        // directive value
                        triggerPoint = TriggerPoint.DirectiveValue;

                        // prepare applicable start position to include value text tag in the intellisense selection to replace
                        applicableToStartPosition = tokens[currentTokenIndex].StartPosition;
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == DothtmlTokenType.OpenTag || IsTagName(currentTokenIndex, tokens))
                    {
                        // element name
                        triggerPoint = TriggerPoint.TagName;
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                        if (currentToken.Type != DothtmlTokenType.OpenTag)
                        {
                            applicableToStartPosition = tokens[GetTagNameStartTokenIndex(currentTokenIndex, tokens)].StartPosition;
                        }
                        combineWithHtmlCompletions = true;
                    }
                    else if (currentToken.Type == DothtmlTokenType.WhiteSpace || currentToken.Type == DothtmlTokenType.Text)
                    {
                        var previousToken = GetPreviousToken(currentTokenIndex, tokens, new[] { DothtmlTokenType.WhiteSpace });

                        if (context.CurrentNode?.Tokens.All(all => all.Type == DothtmlTokenType.WhiteSpace || (all.Type == DothtmlTokenType.Text && string.IsNullOrWhiteSpace(all.Text))) == true)
                        {
                            session.Dismiss();
                            return;
                        }

                        // prepare applicable start position to include current text tag in the intellisense selection to replace
                        if (currentToken.Type == DothtmlTokenType.Text)
                        {
                            applicableToStartPosition = tokens[currentTokenIndex].StartPosition;
                        }

                        if (context.CurrentNode is DothtmlDirectiveNode && previousToken?.Type == DothtmlTokenType.DirectiveName)
                        {
                            // directive value
                            triggerPoint = TriggerPoint.DirectiveValue;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                        }
                        else if (previousToken?.Type == DothtmlTokenType.OpenBinding)
                        {
                            // binding name
                            triggerPoint = TriggerPoint.BindingName;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                        }
                        else if (context.CurrentNode is DothtmlElementNode || context.CurrentNode is DothtmlAttributeNode)
                        {
                            // attribute name
                            triggerPoint = TriggerPoint.TagAttributeName;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                            combineWithHtmlCompletions = sourceProvider.CompletionProviders.OfType<MainTagAttributeNameCompletionProvider>().Single().CombineWithHtmlCompletions;
                        }
                        else if (previousToken?.Type == DothtmlTokenType.SingleQuote || previousToken?.Type == DothtmlTokenType.DoubleQuote || previousToken?.Type == DothtmlTokenType.Equals)
                        {
                            // attribute value
                            triggerPoint = TriggerPoint.TagAttributeValue;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                            combineWithHtmlCompletions = true;
                        }
                    }
                    else if (currentToken.Type == DothtmlTokenType.SingleQuote || currentToken.Type == DothtmlTokenType.DoubleQuote || currentToken.Type == DothtmlTokenType.Equals)
                    {
                        // attribute value
                        triggerPoint = TriggerPoint.TagAttributeValue;
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                        combineWithHtmlCompletions = true;
                    }
                    else if (currentToken.Type == DothtmlTokenType.OpenBinding)
                    {
                        // binding name
                        triggerPoint = TriggerPoint.BindingName;
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == DothtmlTokenType.Colon)
                    {
                        if (context.CurrentNode is DothtmlBindingNode)
                        {
                            // binding value
                            triggerPoint = TriggerPoint.BindingValue;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                        }
                        else
                        {
                            // element name
                            triggerPoint = TriggerPoint.TagName;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                            combineWithHtmlCompletions = true;
                        }
                    }
                    var results = items.OrderBy(v => v.DisplayText).Distinct(CompletionEqualityComparer.Instance).ToList();

                    // handle duplicate sessions (sometimes this method is called twice (e.g. when space key is pressed) so we need to make sure that we'll display only one session
                    lock (activeSessions)
                    {
                        if (activeSessions.Count > 0)
                        {
                            activeSessions.ToList().ForEach(fe => fe.Dismiss());
                        }
                        activeSessions.Add(session);

                        session.Dismissed += (s, a) =>
                        {
                            lock (activeSessions)
                            {
                                activeSessions.Remove((ICompletionSession)s);
                            }
                        };
                        session.Committed += (s, a) =>
                        {
                            lock (activeSessions)
                            {
                                activeSessions.Remove((ICompletionSession)s);
                            }
                        };
                    }

                    if (results.Any())
                    {
                        // show the session
                        var newCompletionSet = new CustomCompletionSet("dotVVM", "dotVVM", FindTokenSpanAtPosition(session, applicableToStartPosition ?? -1), results, null);
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

        public void Dispose()
        {
        }

        internal static int FindCurrentTokenIndex(IList<DothtmlToken> tokens, int cursorPosition)
        {
            var min = 0;
            var max = tokens.Count - 1;

            // use binary search
            while (min <= max)
            {
                var middle = (min + max) / 2;
                if (cursorPosition < tokens[middle].StartPosition)
                {
                    max = middle - 1;
                }
                else if (cursorPosition > tokens[middle].StartPosition + tokens[middle].Length)
                {
                    min = middle + 1;
                }
                else
                {
                    // there may be multiple tokens which meet the condition - choose the first of them
                    while (middle > 0
                        && tokens[middle - 1].StartPosition <= cursorPosition
                        && tokens[middle - 1].StartPosition + tokens[middle - 1].Length >= cursorPosition)
                    {
                        middle--;
                    }

                    return middle;
                }
            }
            return -1;
        }

        private static CustomCompletionSet MergeCompletionSets(IList<CompletionSet> completionSets, CustomCompletionSet newCompletions)
        {
            var htmlCompletionsSet = completionSets.First();

            // if we are in an element with tagPrefix, VS adds all HTML elements with the same prefix in the completion - we don't want it
            var originalCompletions = htmlCompletionsSet.Completions.Where(c => !c.DisplayText.Contains(":"));

            // merge
            var mergedCompletionSet = new CustomCompletionSet(
                htmlCompletionsSet.Moniker,
                htmlCompletionsSet.DisplayName,
                htmlCompletionsSet.ApplicableTo,
                newCompletions.Completions.Concat(originalCompletions).OrderBy(n => n.DisplayText).Distinct(CompletionEqualityComparer.Instance),
                htmlCompletionsSet.CompletionBuilders);

            completionSets.Remove(htmlCompletionsSet);
            return mergedCompletionSet;
        }

        private DothtmlToken GetPreviousToken(int currentTokenIndex, IList<DothtmlToken> tokens, params DothtmlTokenType[] skipTypes)
        {
            if (currentTokenIndex == 0)
                return null;

            while (skipTypes?.Contains(tokens[--currentTokenIndex].Type) == true)
            {
                if (currentTokenIndex == 0)
                    return null;
            }
            return tokens[currentTokenIndex];
        }

        private bool IsTagName(int currentTokenIndex, IList<DothtmlToken> tokens)
        {
            while (currentTokenIndex > 0)
            {
                var token = tokens[currentTokenIndex--];
                if (token.Type == DothtmlTokenType.OpenTag)
                    return true;
                if (token.Type == DothtmlTokenType.Colon || token.Type == DothtmlTokenType.Text)
                    continue;
                return false;
            }
            return false;
        }

        private int GetTagNameStartTokenIndex(int currentTokenIndex, IList<DothtmlToken> tokens)
        {
            while (currentTokenIndex > 0)
            {
                var token = tokens[currentTokenIndex--];
                if (token.Type == DothtmlTokenType.OpenTag)
                    return currentTokenIndex + 2;
                if (token.Type == DothtmlTokenType.Colon || token.Type == DothtmlTokenType.Text)
                    continue;
                break;
            }
            return -1;
        }

        private ITrackingSpan FindTokenSpanAtPosition(ICompletionSession session, int startPosition = -1)
        {
            var currentPoint = session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot);
            startPosition = startPosition > -1 ? startPosition : currentPoint.Position;
            var length = currentPoint.Position >= startPosition ? currentPoint.Position - startPosition : 0;
            return currentPoint.Snapshot.CreateTrackingSpan(startPosition, length, SpanTrackingMode.EdgeInclusive);
        }

        internal class SessionInfo
        {
            public DothtmlCompletionContext DothtmlCompletionContext { get; set; }

            public ICompletionSession CompletionSession { get; set; }
        }
    }
}