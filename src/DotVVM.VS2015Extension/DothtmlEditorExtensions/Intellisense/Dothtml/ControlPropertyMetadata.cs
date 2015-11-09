using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml
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