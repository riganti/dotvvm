using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Diagnostics;
using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    public class RwHtmlFormatCommandHandler : BaseCommandTarget
    {
        
        public override Guid CommandGroupId
        {
            get
            {
                return typeof(VSConstants.VSStd2KCmdID).GUID;
            }
        }

        public override uint[] CommandIds
        {
            get
            {
                return new[] { (uint)VSConstants.VSStd2KCmdID.FORMATSELECTION, (uint)VSConstants.VSStd2KCmdID.FORMATDOCUMENT };
            }
        }

        public RwHtmlFormatCommandHandler(IVsTextView textViewAdapter, ITextView textView) : base(textViewAdapter, textView)
        {
        }

        protected override bool Execute(uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, IOleCommandTarget nextCommandTarget)
        {
            // TODO: call the original command and fix casing in Redwood controls
            Trace.WriteLine("Format command applied!");
            return true;
        }
    }
}