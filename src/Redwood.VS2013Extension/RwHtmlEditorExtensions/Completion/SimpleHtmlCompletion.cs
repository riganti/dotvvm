using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.Html.Editor.Intellisense;
using Microsoft.Web.Editor;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completion
{
    public class SimpleHtmlCompletion : HtmlCompletion
    {
        private static ImageSource _glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);

        public SimpleHtmlCompletion(string displayText, ICompletionSession session)
            : base(displayText, displayText, string.Empty, _glyph, HtmlIconAutomationText.AttributeIconText, session)
        {
            
        }

        public SimpleHtmlCompletion(string displayText, string description, ICompletionSession session)
            : base(displayText, displayText, description, _glyph, HtmlIconAutomationText.AttributeIconText, session)
        {
            
        }
    }
}