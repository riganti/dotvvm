using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public abstract class DirectiveNameHtmlCompletionProviderBase : IRwHtmlCompletionProvider
    {
        public TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.DirectiveName; }
        }

        public abstract IEnumerable<SimpleRwHtmlCompletion> GetItems(RwHtmlCompletionContext context);
    }
}
