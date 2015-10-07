using DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Commands.GotoDefinition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Threading;

namespace DotVVM.VS2015Extension.Bases.Commands
{
    public abstract class BaseCommandTarget : IOleCommandTarget
    {
        private IOleCommandTarget nextCommandHandler;

        public ITextView TextView { get; }

        public IVsTextView TextViewAdapter { get; }
        public VisualStudioWorkspace Workspace { get; set; }
        public WorkspaceHelper WorkspaceHelper { get; set; }

        protected BaseCommandTarget(IVsTextView textViewAdapter, ITextView textView, BaseHandlerProvider provider)
        {
            TextViewAdapter = textViewAdapter;
            TextView = textView;
            Workspace = provider.VsWorkspace;
            WorkspaceHelper = new WorkspaceHelper() { Workspace = Workspace };

            // Add the target later to make sure it makes it in before other command handlers (thanks to Mads Kristensen - https://github.com/madskristensen/WebEssentials2013/blob/fa001f0737b0ab0a76a3e9fac9d89e6b722d70f3/EditorExtensions/Shared/Commands/CommandTargetBase.cs)
            Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                ErrorHandler.ThrowOnFailure(textViewAdapter.AddCommandFilter(this, out nextCommandHandler));
            }, DispatcherPriority.ApplicationIdle);
        }

        public abstract Guid CommandGroupId { get; }

        public abstract uint[] CommandIds { get; }

        public virtual int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
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

        public int Exec(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == CommandGroupId && CommandIds.Contains(nCmdId))
            {
                var commandHandler = new NextIOleCommandTarget(nextCommandHandler);
                var result = Execute(nCmdId, nCmdexecopt, pvaIn, pvaOut, commandHandler);
                if (result)
                {
                    return VSConstants.S_OK;
                }
            }

            return nextCommandHandler.Exec(ref pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
        }

        protected abstract bool Execute(uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, NextIOleCommandTarget nextCommandTarget);
    }
}