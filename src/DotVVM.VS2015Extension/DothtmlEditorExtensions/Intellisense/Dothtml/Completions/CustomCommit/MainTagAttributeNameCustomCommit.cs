using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions.CustomCommit
{
    internal class MainTagAttributeNameCustomCommit : Base.CustomCommit
    {
        public MainTagAttributeNameCustomCommit(DothtmlCompletionContext context) : base(context)
        {
        }

        protected override void CommitCore(DothtmlCompletionContext context, Completion selectedCompletion)
        {
            var session = context.CompletionSession;

            session.Commit();

            if (session.TextView.TextBuffer.CheckEditAccess())
            {
                using (var edit = session.TextView.TextBuffer.CreateEdit())
                {
                    edit.Insert(session.TextView.Caret.Position.BufferPosition, "=\"\"");
                    edit.Apply();
                    session.TextView.Caret.MoveTo(session.TextView.Caret.Position.BufferPosition - 1);
                    CompletionTriggerRequested = true;
                }
            }
        }
    }
}