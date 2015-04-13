using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Diagnostics;
using System;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Redwood.Framework.Parser;
using Redwood.Framework.Parser.RwHtml.Parser;
using Redwood.Framework.Parser.RwHtml.Tokenizer;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

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
            var groupId = CommandGroupId;
            if (nextCommandTarget.Exec(ref groupId, nCmdID, nCmdexecopt, pvaIn, pvaOut) == VSConstants.S_OK)
            {
                // parse the content
                var tokenizer = new RwHtmlTokenizer();
                tokenizer.Tokenize(new StringReader(TextView.TextSnapshot.GetText()));
                var parser = new RwHtmlParser();
                var node = parser.Parse(tokenizer.Tokens);
                
                var metadataControlResolver = TextView.Properties.GetProperty<RwHtmlCompletionSourceProvider>(null).MetadataControlResolver;
                try
                {
                    CompletionHelper.DTE.UndoContext.Open("Format RWHTML document");

                    // fix the casing of all elements
                    foreach (var element in node.EnumerateNodes().OfType<RwHtmlElementNode>())
                    {
                        FixElement(metadataControlResolver, TextView.TextBuffer, element);
                    }
                }
                finally
                {
                    CompletionHelper.DTE.UndoContext.Close();
                }
            }

            return true;
        }

        private void FixElement(MetadataControlResolver metadataControlResolver, ITextBuffer textBuffer, RwHtmlElementNode element)
        {
            // fix element name
            var metadata = metadataControlResolver.GetMetadata(element.FullTagName);
            if (metadata != null)
            {
                // we have found the metadata for the control
                if (metadata.Name != element.FullTagName)
                {
                    // the used name differs from the correct, fix the tag name
                    var edit = textBuffer.CreateEdit();
                    edit.Replace(element.TagPrefixToken.StartPosition, element.TagPrefixToken.Length, metadata.TagPrefix);
                    edit.Replace(element.TagNameToken.StartPosition, element.TagNameToken.Length, metadata.TagName);
                    edit.Apply();
                }
            }

            // fix attribute names
            foreach (var attribute in element.Attributes)
            {
                var property = metadata.GetProperty(attribute.AttributeName);
                if (property != null && property.Name != attribute.AttributeName)
                {
                    // the used name differs from the correct, fix the tag name
                    var edit = textBuffer.CreateEdit();
                    edit.Replace(attribute.AttributeNameToken.StartPosition, attribute.AttributeNameToken.Length, property.Name);
                    edit.Apply();
                }
            }

            // fix property elements
            foreach (var child in element.Content.OfType<RwHtmlElementNode>())
            {
                var property = metadata.GetProperty(child.FullTagName);
                if (property != null && property.IsElement && property.Name != child.FullTagName)
                {
                    // the used name differs from the correct, fix the tag name
                    var edit = textBuffer.CreateEdit();
                    edit.Replace(element.TagNameToken.StartPosition, element.TagNameToken.Length, property.Name);
                    edit.Apply();
                }
            }
        }
    }
}