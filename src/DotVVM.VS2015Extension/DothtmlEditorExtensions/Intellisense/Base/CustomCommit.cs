using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
{
    public abstract class CustomCommit : ICustomCommit
    {
        private DothtmlCompletionContext context;
        private Completion selectedCompletion;

        public CustomCommit(DothtmlCompletionContext context)
        {
            this.context = context;

            if (context.CompletionSession?.SelectedCompletionSet != null)
            {
                this.selectedCompletion = context.CompletionSession.SelectedCompletionSet.SelectionStatus.IsSelected
                    ? context.CompletionSession.SelectedCompletionSet.SelectionStatus.Completion
                    : context.CompletionSession.SelectedCompletionSet.Completions.FirstOrDefault();
            }
        }

        public bool CompletionTriggerRequested { get; set; }

        public void Commit()
        {
            CommitCore(context, selectedCompletion);
        }

        protected abstract void CommitCore(DothtmlCompletionContext context, Completion completion);
    }
}