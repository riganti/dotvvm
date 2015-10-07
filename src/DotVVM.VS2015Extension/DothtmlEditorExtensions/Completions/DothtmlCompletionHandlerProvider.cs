using DotVVM.VS2015Extension.Bases;
using DotVVM.VS2015Extension.Configuration;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Threading;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Dothtml Completion Handler")]
    [ContentType(ContentTypeDefinitions.DothtmlContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class DothtmlCompletionHandlerProvider : BaseHandlerProvider, IVsTextViewCreationListener
    {
        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                textView.Properties.GetOrCreateSingletonProperty(() => new DothtmlCompletionCommandHandler(textViewAdapter, textView, this));
                textView.Properties.GetOrCreateSingletonProperty(() => new DothtmlFormatCommandHandler(textViewAdapter, textView, this));

                var tempSession = CompletionBroker.CreateCompletionSession(textView, textView.TextSnapshot.CreateTrackingPoint(0, Microsoft.VisualStudio.Text.PointTrackingMode.Negative), true);
                tempSession.Start();
                tempSession.Dismiss();
            }, DispatcherPriority.ApplicationIdle);
        }
    }
}