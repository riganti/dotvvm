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

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Dothtml Completion Handler")]
    [ContentType(DothtmlContentTypeDefinitions.DothtmlContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class DothtmlCompletionHandlerProvider : IVsTextViewCreationListener
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
            
            textView.Properties.GetOrCreateSingletonProperty(() => new DothtmlCompletionCommandHandler(textViewAdapter, textView, this));
            textView.Properties.GetOrCreateSingletonProperty(() => new DothtmlFormatCommandHandler(textViewAdapter, textView));
        }
    }
}