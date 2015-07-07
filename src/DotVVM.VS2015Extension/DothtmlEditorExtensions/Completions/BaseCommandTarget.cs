using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Linq;
using Microsoft.VisualStudio;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Windows.Threading;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    public abstract class BaseCommandTarget : IOleCommandTarget
    {
        private IOleCommandTarget nextCommandHandler;
        private ITextView textView;
        private IVsTextView textViewAdapter;

        public ITextView TextView
        {
            get { return textView; }
        }

        public IVsTextView TextViewAdapter
        {
            get { return textViewAdapter; }
        }

        public BaseCommandTarget(IVsTextView textViewAdapter, ITextView textView)
        {
            this.textViewAdapter = textViewAdapter;
            this.textView = textView;

            // Add the target later to make sure it makes it in before other command handlers (thanks to Mads Kristensen - https://github.com/madskristensen/WebEssentials2013/blob/fa001f0737b0ab0a76a3e9fac9d89e6b722d70f3/EditorExtensions/Shared/Commands/CommandTargetBase.cs) 
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(this, out nextCommandHandler));
            }, DispatcherPriority.ApplicationIdle);
        }

        public abstract Guid CommandGroupId { get; }

        public abstract uint[] CommandIds { get; }



        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == CommandGroupId)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    if (CommandIds.Contains(prgCmds[i].cmdID))
                    {
                        prgCmds[0].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (uint)OLECMDF.OLECMDF_ENABLED;
                        return VSConstants.S_OK;
                    }
                }
            }
            return nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == CommandGroupId && CommandIds.Contains(nCmdID))
            {
                var result = Execute(nCmdID, nCmdexecopt, pvaIn, pvaOut, nextCommandHandler);
                if (result)
                {
                    return VSConstants.S_OK;
                }
            }

            return nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        protected abstract bool Execute(uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, IOleCommandTarget nextCommandTarget);
    }
}