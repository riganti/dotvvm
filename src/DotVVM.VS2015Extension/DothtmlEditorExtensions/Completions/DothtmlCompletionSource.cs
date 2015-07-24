using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EnvDTE80;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    public class DothtmlCompletionSource : ICompletionSource
    {
        private readonly DothtmlCompletionSourceProvider sourceProvider;
        private readonly ITextBuffer textBuffer;
        private readonly VisualStudioWorkspace workspace;
        private readonly IGlyphService glyphService;
        private readonly DTE2 dte;
        private readonly DotvvmConfigurationProvider configurationProvider;
        private readonly DothtmlClassifier classifier;
        private readonly DothtmlParser parser;

        public MetadataControlResolver MetadataControlResolver { get; private set; }

        private static List<ICompletionSession> activeSessions = new List<ICompletionSession>(); 

        public DothtmlCompletionSource(DothtmlCompletionSourceProvider sourceProvider, DothtmlParser parser, 
            DothtmlClassifier classifier, ITextBuffer textBuffer, VisualStudioWorkspace workspace, 
            IGlyphService glyphService, DTE2 dte, DotvvmConfigurationProvider configurationProvider,
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
            this.MetadataControlResolver = metadataControlResolver;
        }

        public DothtmlCompletionContext GetCompletionContext()
        {
            var context = new DothtmlCompletionContext()
            {
                Tokens = classifier.Tokens,
                Parser = parser,
                Tokenizer = classifier.Tokenizer,
                RoslynWorkspace = workspace,
                GlyphService = glyphService,
                DTE = dte,
                Configuration = configurationProvider.GetConfiguration(dte.ActiveDocument.ProjectItem.ContainingProject),
                MetadataControlResolver = MetadataControlResolver
            };
            parser.Parse(classifier.Tokens);
            return context;
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
                    // prepare the context
                    var items = Enumerable.Empty<SimpleDothtmlCompletion>();
                    var context = GetCompletionContext();
                    context.CurrentTokenIndex = classifier.Tokens.IndexOf(currentToken);
                    context.CurrentNode = parser.Root.FindNodeByPosition(session.TextView.Caret.Position.BufferPosition.Position - 1);
                    var combineWithHtmlCompletions = false;

                    TriggerPoint triggerPoint = TriggerPoint.None;
                    if (currentToken.Type == DothtmlTokenType.DirectiveStart)
                    {
                        // directive name completion
                        triggerPoint = TriggerPoint.DirectiveName;
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                    }
                    else if (currentToken.Type == DothtmlTokenType.WhiteSpace)
                    {
                        if (context.CurrentNode is DothtmlDirectiveNode && context.CurrentTokenIndex >= 2 && tokens[context.CurrentTokenIndex - 2].Type == DothtmlTokenType.DirectiveStart)
                        {
                            // directive value
                            triggerPoint = TriggerPoint.DirectiveValue;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                        }
                        else if (context.CurrentNode is DothtmlElementNode || context.CurrentNode is DothtmlAttributeNode)
                        {
                            // attribute name
                            triggerPoint = TriggerPoint.TagAttributeName;
                            items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                            combineWithHtmlCompletions = sourceProvider.CompletionProviders.OfType<MainTagAttributeNameCompletionProvider>().Single().CombineWithHtmlCompletions;
                        }
                    }
                    else if (currentToken.Type == DothtmlTokenType.OpenTag)
                    {
                        // element name
                        triggerPoint = TriggerPoint.TagName;
                        items = sourceProvider.CompletionProviders.Where(p => p.TriggerPoint == triggerPoint).SelectMany(p => p.GetItems(context));
                        combineWithHtmlCompletions = true;
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
                    
                    // show the session
                    if (!results.Any())
                    {
                        session.Dismiss();
                    }
                    else
                    {
                        // handle duplicate sessions (sometimes this method is called twice (e.g. when space key is pressed) so we need to make sure that we'll display only one session
                        lock (activeSessions)
                        {
                            if (activeSessions.Count > 0)
                            {
                                session.Dismiss();
                                return;
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

                        // show the session
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

        public void Dispose()
        {
        }

    }
}
