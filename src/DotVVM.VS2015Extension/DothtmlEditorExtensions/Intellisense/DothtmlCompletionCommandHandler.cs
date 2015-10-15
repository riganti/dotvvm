using DotVVM.VS2015Extension.Bases.Commands;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense
{
    internal class DothtmlCompletionCommandHandler : IOleCommandTarget
    {
        private NextIOleCommandTarget<DothtmlCompletionCommandHandler> nextCommandHandler;
        private ITextView textView;
        private DothtmlCompletionHandlerProvider provider;

        private ICompletionSession session;
        private DothtmlCompletionContext context;

        internal DothtmlCompletionCommandHandler(IVsTextView textViewAdapter, ITextView textView, DothtmlCompletionHandlerProvider provider)
        {
            this.textView = textView;
            this.provider = provider;

            //add the command to the chain and get following command
            nextCommandHandler = new NextIOleCommandTarget<DothtmlCompletionCommandHandler>(textViewAdapter, this);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(provider.ServiceProvider))
            {
                return nextCommandHandler.Execute(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            }

            //make a copy of this so we can look at it after forwarding some commands
            uint commandId = nCmdId;
            char typedChar = char.MinValue;
            bool handled = false;
            bool completionRequested = false;

            lock (DothtmlCompletionSource.activeSessions)
            {
                session = DothtmlCompletionSource.activeSessions.FirstOrDefault();
            }

            //make sure the input is a char before getting it
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdId == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character
            if (nCmdId == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdId == (uint)VSConstants.VSStd2KCmdID.TAB
                || (char.IsWhiteSpace(typedChar)
                //|| char.IsPunctuation(typedChar)
                || typedChar == '='))
            {
                //check for a a selection
                if (session != null && session.IsStarted && !session.IsDismissed)
                {
                    var textView = session.TextView;
                    // if TAB is pressed, select the best match to commit
                    bool selectionTabOverrule = nCmdId == (uint)VSConstants.VSStd2KCmdID.TAB && !session.SelectedCompletionSet.SelectionStatus.IsSelected;

                    //if the selection is fully selected, commit the current session
                    if (session.SelectedCompletionSet.SelectionStatus.IsSelected || selectionTabOverrule)
                    {
                        var customCommit = (session.SelectedCompletionSet.SelectionStatus.Completion as SimpleDothtmlCompletion)?.CustomCommit;
                        if (customCommit != null)
                        {
                            customCommit.Commit();
                            completionRequested = customCommit.CompletionTriggerRequested;
                        }
                        else
                        {
                            session.Commit();
                        }

                        var prevChar = textView.TextBuffer.CurrentSnapshot[textView.Caret.Position.BufferPosition];
                        if (typedChar != '=' || prevChar == '\"' || prevChar == '\'')
                        {
                            handled = true;
                        }
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        if (!session.IsDismissed)
                        {
                            session.Dismiss();
                        }
                    }
                }
            }

            // pass along the command so the char is added to the buffer
            int retVal = 0;
            if (!handled)
            {
                retVal = nextCommandHandler.Execute(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            }
            if ((nCmdId == (uint)VSConstants.VSStd2KCmdID.RETURN || nCmdId == (uint)VSConstants.VSStd2KCmdID.TAB) || completionRequested ||
                (!typedChar.Equals(char.MinValue) && IsTriggerChar(typedChar)))
            {
                if (session == null || session.IsDismissed) // If there is no active session, bring up completion
                {
                    this.TriggerCompletion();
                }
                else     //the completion session is already active, so just filter
                {
                    session.Filter();
                }
                handled = true;
            }
            else if (commandId == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   // redo the filter if there is a deletion
                || commandId == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (session != null && !session.IsDismissed)
                    session.Filter();
                handled = true;
            }

            if (handled) return VSConstants.S_OK;
            return retVal;
        }

        private bool TriggerCompletion()
        {
            //the caret must be in a non-projection location
            SnapshotPoint? caretPoint =
            textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);
            if (!caretPoint.HasValue)
            {
                return false;
            }

            session = provider.CompletionBroker.CreateCompletionSession(textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            //subscribe to the Dismissed event on the session
            session.Dismissed += this.OnSessionDismissed;
            session.Start();

            return true;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            if (sender is ICompletionSession)
            {
                ((ICompletionSession)sender).Dismissed -= this.OnSessionDismissed;
            }
            session = null;
        }

        private bool IsTriggerChar(char typedChar)
        {
            return typedChar == '@' || typedChar == '{' || typedChar == ' ';
        }
    }
}