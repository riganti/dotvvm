using DotVVM.VS2015Extension.Bases;
using DotVVM.VS2015Extension.Configuration;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Threading;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Commands.GotoDefinition
{
    [Export(typeof(IVsTextViewCreationListener)),
    Name("Dotvvm GotoDefinition Handler Register"),
    ContentType(ContentTypeDefinitions.DothtmlContentType),
    TextViewRole(PredefinedTextViewRoles.Interactive)]
    public class DothtmlGotoDefinitionHandlerProvider : BaseHandlerProvider, IVsTextViewCreationListener
    {
        public void VsTextViewCreated(IVsTextView textViewAdapter)

        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                textView.Properties.GetOrCreateSingletonProperty(() => new DothtmlGotoDefinitionCommandHandler(textViewAdapter, textView, this));

                var tempSession = CompletionBroker.CreateCompletionSession(textView, textView.TextSnapshot.CreateTrackingPoint(0, Microsoft.VisualStudio.Text.PointTrackingMode.Negative), true);
                tempSession.Start();
                tempSession.Dismiss();
            }, DispatcherPriority.ApplicationIdle);
        }
    }
}