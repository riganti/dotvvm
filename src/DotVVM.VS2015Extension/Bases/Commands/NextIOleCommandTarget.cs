using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace DotVVM.VS2015Extension.Bases.Commands
{
    public class NextIOleCommandTarget
    {
        protected IOleCommandTarget nextCommandTarget;

        protected NextIOleCommandTarget()
        {
        }

        public NextIOleCommandTarget(IOleCommandTarget nextCommandTarget)
        {
            this.nextCommandTarget = nextCommandTarget;
        }

        /// <summary>
        /// Executes next command. Exceptions are cached. Error result is zero.
        /// </summary>
        public int Execute(ref Guid pguidCmdGroup, uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            try
            {
                return nextCommandTarget.Exec(pguidCmdGroup, nCmdId, nCmdexecopt, pvaIn, pvaOut);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            // execute query
            return nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }

    public class NextIOleCommandTarget<T> : NextIOleCommandTarget where T : IOleCommandTarget
    {
        /// <summary>
        /// Provides next IOleCommandTarget and adds <param name="myCommand">myCommand</param> to command chain.
        /// </summary>
        /// <param name="textViewAdapter"></param>
        /// <param name="myCommand">Your command to add to chain.</param>
        public NextIOleCommandTarget(IVsTextView textViewAdapter, T myCommand)
        {
            textViewAdapter.AddCommandFilter(myCommand, out nextCommandTarget);
        }
    }
}