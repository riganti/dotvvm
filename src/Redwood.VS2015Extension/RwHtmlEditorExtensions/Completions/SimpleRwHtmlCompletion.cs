using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions
{
    public class SimpleRwHtmlCompletion : Completion
    {

        public SimpleRwHtmlCompletion(string displayText)
            : this(displayText, displayText)
        {
            
        }

        public SimpleRwHtmlCompletion(string displayText, string completionText)
            : base(displayText, completionText, string.Empty, null, displayText)
        {

        }

        public SimpleRwHtmlCompletion(string displayText, string completionText, ImageSource glyph)
            : base(displayText, completionText, string.Empty, glyph, displayText)
        {

        }

    }
}