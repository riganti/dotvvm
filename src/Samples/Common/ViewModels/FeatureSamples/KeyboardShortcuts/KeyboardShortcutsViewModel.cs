using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.KeyboardShortcuts
{
    public class KeyboardShortcutsViewModel : DotvvmViewModelBase
    {
        public bool DocumentShortcutEnabled { get; set; } = true;
        public int DocumentShortcutCount { get; set; }
        public int OuterDialogShortcutCount { get; set; }
        public int InnerDialogShortcutCount { get; set; }
        public bool OuterDialogOpen { get; set; }
        public bool InnerDialogOpen { get; set; }
        public string Draft { get; set; } = "";
        public string SubmittedText { get; set; } = "";
        public int SubmissionCount { get; set; }

        public void HandleDocumentShortcut() => DocumentShortcutCount++;

        public void HandleOuterDialogShortcut() => OuterDialogShortcutCount++;

        public void HandleInnerDialogShortcut() => InnerDialogShortcutCount++;

        public void SubmitDraft()
        {
            SubmittedText = Draft;
            SubmissionCount++;
        }
    }
}
