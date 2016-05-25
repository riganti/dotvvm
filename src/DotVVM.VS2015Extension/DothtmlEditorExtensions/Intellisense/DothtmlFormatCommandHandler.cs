using DotVVM.VS2015Extension.Bases;
using DotVVM.VS2015Extension.Bases.Commands;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense
{
    public class DothtmlFormatCommandHandler : BaseCommandTarget
    {
        public DothtmlFormatCommandHandler(IVsTextView textViewAdapter, ITextView textView, BaseHandlerProvider provider) : base(textViewAdapter, textView, provider)
        {
        }

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

        protected override bool Execute(uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, NextIOleCommandTarget nextCommandTarget)
        {
            var groupId = CommandGroupId;
            if (nextCommandTarget.Execute(ref groupId, nCmdId, nCmdexecopt, pvaIn, pvaOut) == VSConstants.S_OK)
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
                metadataControlResolver.ReloadAllControls(completionSource.GetCompletionContext(null));

                try
                {
                    DTEHelper.UndoContext.Open("Format Dothtml document");
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
                    DTEHelper.UndoContext.Close();
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
                    edit.SetRange(element.TagPrefixNode.StartPosition, element.TagPrefixNode.Length, metadata.TagPrefix);
                    edit.SetRange(element.TagNameNode.StartPosition, element.TagNameNode.Length, metadata.TagName);

                    if (element.CorrespondingEndTag != null)
                    {
                        edit.SetRange(element.CorrespondingEndTag.TagPrefixNode.StartPosition, element.CorrespondingEndTag.TagPrefixNode.Length, metadata.TagPrefix);
                        edit.SetRange(element.CorrespondingEndTag.TagNameNode.StartPosition, element.CorrespondingEndTag.TagNameNode.Length, metadata.TagName);
                    }
                }

                // fix attribute names
                foreach (var attribute in element.Attributes)
                {
                    var property = metadata.GetProperty(attribute.AttributeName);
                    if (property != null && property.Name != attribute.AttributeName)
                    {
                        // the used name differs from the correct, fix the tag name
                        edit.SetRange(attribute.AttributeNameNode.StartPosition, attribute.AttributeNameNode.Length, property.Name);
                    }
                }

                // fix property elements
                foreach (var child in element.Content.OfType<DothtmlElementNode>())
                {
                    var property = metadata.GetProperty(child.FullTagName);
                    if (property != null && property.IsElement && property.Name != child.FullTagName)
                    {
                        // the used name differs from the correct, fix the tag name
                        edit.SetRange(child.TagPrefixNode.StartPosition, child.TagPrefixNode.Length, property.Name);
                        if (child.CorrespondingEndTag != null)
                        {
                            edit.SetRange(child.CorrespondingEndTag.TagNameNode.StartPosition, child.CorrespondingEndTag.TagNameNode.Length, property.Name);
                        }
                    }
                }
            }
        }
    }
}