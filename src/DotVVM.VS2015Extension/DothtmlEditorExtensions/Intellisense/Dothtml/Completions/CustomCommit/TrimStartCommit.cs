using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions.CustomCommit
{
    internal class TrimStartCommit : Base.CustomCommit
    {
        public TrimStartCommit(DothtmlCompletionContext context) : base(context)
        {
        }

        protected override void CommitCore(DothtmlCompletionContext context, Completion selectedCompletion)
        {
            var session = context.CompletionSession;

            var currentToken = context.Tokens[context.CurrentTokenIndex];
            var currentPosition = session.TextView.Caret.Position.BufferPosition.Position;
            if (currentToken.StartPosition < currentPosition)
            {
                using (var edit = session.TextView.TextBuffer.CreateEdit(EditOptions.DefaultMinimalChange, null, this))
                {
                    edit.Replace(currentToken.StartPosition, currentPosition - currentToken.StartPosition, selectedCompletion.InsertionText);
                    edit.Apply();
                    session.Dismiss();
                }
            }
            else
            {
                session.Commit();
            }
        }
    }
}