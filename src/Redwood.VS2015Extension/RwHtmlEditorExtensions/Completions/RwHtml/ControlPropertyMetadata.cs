using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public class ControlPropertyMetadata
    {
        public string Name { get; set; }
        public bool IsTemplate { get; set; }
        public bool AllowBinding { get; set; }
        public bool AllowHardCodedValue { get; set; }
        public bool IsElement { get; set; }
        public bool AllowHtmlContent { get; set; }
        public ITypeSymbol Type { get; set; }

    }
}