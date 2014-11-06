using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Html.Editor.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completion
{
    [HtmlCompletionProvider(CompletionType.Children)]
    [ContentType(RwHtmlContentTypeDefinitions.RwHtmlContentType)]
    public class RwHtmlElementNameCompletion : IHtmlCompletionListProvider
    {
        // TODO: this doesn't work yet - don't know why
        public IList<HtmlCompletion> GetEntries(HtmlCompletionContext context)
        {
            // TODO: load Redwood configuration, analyze namespaces and load control metadata (using ControlResolver)

            return new HtmlCompletion[]
            {
                new SimpleHtmlCompletion("Literal", context.Session),
                new SimpleHtmlCompletion("Button", context.Session),
                new SimpleHtmlCompletion("LinkButton", context.Session),
                new SimpleHtmlCompletion("Repeater", context.Session),
                new SimpleHtmlCompletion("IntegrationScripts", context.Session)
            };
        }

        public CompletionType CompletionType
        {
            get { return CompletionType.Children; }
        }
    }
}
