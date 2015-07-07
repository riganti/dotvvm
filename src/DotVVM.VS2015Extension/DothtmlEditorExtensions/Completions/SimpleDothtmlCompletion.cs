using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    public class SimpleDothtmlCompletion : Completion
    {

        public SimpleDothtmlCompletion(string displayText)
            : this(displayText, displayText)
        {
            
        }

        public SimpleDothtmlCompletion(string displayText, string completionText)
            : base(displayText, completionText, string.Empty, null, displayText)
        {

        }

        public SimpleDothtmlCompletion(string displayText, string completionText, ImageSource glyph)
            : base(displayText, completionText, string.Empty, glyph, displayText)
        {

        }

    }
}