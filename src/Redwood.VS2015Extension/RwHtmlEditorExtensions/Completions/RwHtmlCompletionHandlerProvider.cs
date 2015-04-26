using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.Windows.Threading;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("RWHTML Completion Handler")]
    [ContentType(RwHtmlContentTypeDefinitions.RwHtmlContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class RwHtmlCompletionHandlerProvider : IVsTextViewCreationListener
    {

        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }
        
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                var tempSession = CompletionBroker.CreateCompletionSession(textView, textView.TextSnapshot.CreateTrackingPoint(0, Microsoft.VisualStudio.Text.PointTrackingMode.Negative), true);
                tempSession.Start();
                tempSession.Dismiss();
            }, DispatcherPriority.ApplicationIdle);
            
            textView.Properties.GetOrCreateSingletonProperty(() => new RwHtmlCompletionCommandHandler(textViewAdapter, textView, this));
            textView.Properties.GetOrCreateSingletonProperty(() => new RwHtmlFormatCommandHandler(textViewAdapter, textView));
        }
    }
}