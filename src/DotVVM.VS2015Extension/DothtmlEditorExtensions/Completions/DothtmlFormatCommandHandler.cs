using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Diagnostics;
using System;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;
using System.Text;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    public class DothtmlFormatCommandHandler : BaseCommandTarget
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

        public DothtmlFormatCommandHandler(IVsTextView textViewAdapter, ITextView textView) : base(textViewAdapter, textView)
        {
        }

        protected override bool Execute(uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, IOleCommandTarget nextCommandTarget)
        {
            var groupId = CommandGroupId;
            if (nextCommandTarget.Exec(ref groupId, nCmdID, nCmdexecopt, pvaIn, pvaOut) == VSConstants.S_OK)
            {
                // parse the content
                var tokenizer = new DothtmlTokenizer();
                var text = TextView.TextSnapshot.GetText();
                tokenizer.Tokenize(new StringReader(text));
                var parser = new DothtmlParser();
                var node = parser.Parse(tokenizer.Tokens);

                // prepare the metadata control resolver
                var completionSource = TextView.TextBuffer.Properties.GetProperty<DothtmlCompletionSource>(typeof(DothtmlCompletionSource));
                var metadataControlResolver = completionSource.MetadataControlResolver;
                metadataControlResolver.ReloadAllControls(completionSource.GetCompletionContext());

                try
                {
                    CompletionHelper.DTE.UndoContext.Open("Format Dothtml document");
                    var edit = TextView.TextBuffer.CreateEdit(EditOptions.None, null, null);

                    // fix the casing of all elements
                    var editText = new StringBuilder(text);
                    foreach (var element in node.EnumerateNodes().OfType<DothtmlElementNode>())
                    {
                        FixElement(editText, metadataControlResolver, TextView.TextBuffer, element);
                    }
                    edit.Replace(0, editText.Length, editText.ToString());
                    edit.Apply();
                }
                finally
                {
                    CompletionHelper.DTE.UndoContext.Close();
                }
            }

            return true;
        }

        private void FixElement(StringBuilder edit, MetadataControlResolver metadataControlResolver, ITextBuffer textBuffer, DothtmlElementNode element)
        {
            // fix element name
            var metadata = metadataControlResolver.GetMetadata(element.FullTagName);
            if (metadata != null)
            {
                // we have found the metadata for the control
                if (metadata.FullTagName != element.FullTagName)
                {
                    // the used name differs from the correct, fix the tag name
                    edit.SetRange(element.TagPrefixToken.StartPosition, element.TagPrefixToken.Length, metadata.TagPrefix);
                    edit.SetRange(element.TagNameToken.StartPosition, element.TagNameToken.Length, metadata.TagName);

                    if (element.CorrespondingEndTag != null)
                    {
                        edit.SetRange(element.CorrespondingEndTag.TagPrefixToken.StartPosition, element.CorrespondingEndTag.TagPrefixToken.Length, metadata.TagPrefix);
                        edit.SetRange(element.CorrespondingEndTag.TagNameToken.StartPosition, element.CorrespondingEndTag.TagNameToken.Length, metadata.TagName);
                    }
                }

                // fix attribute names
                foreach (var attribute in element.Attributes)
                {
                    var property = metadata.GetProperty(attribute.AttributeName);
                    if (property != null && property.Name != attribute.AttributeName)
                    {
                        // the used name differs from the correct, fix the tag name
                        edit.SetRange(attribute.AttributeNameToken.StartPosition, attribute.AttributeNameToken.Length, property.Name);
                    }
                }

                // fix property elements
                foreach (var child in element.Content.OfType<DothtmlElementNode>())
                {
                    var property = metadata.GetProperty(child.FullTagName);
                    if (property != null && property.IsElement && property.Name != child.FullTagName)
                    {
                        // the used name differs from the correct, fix the tag name
                        edit.SetRange(child.TagNameToken.StartPosition, child.TagNameToken.Length, property.Name);
                        if (child.CorrespondingEndTag != null)
                        {
                            edit.SetRange(child.CorrespondingEndTag.TagNameToken.StartPosition, child.CorrespondingEndTag.TagNameToken.Length, property.Name);
                        }
                    }
                }
            }
        }
    }
}