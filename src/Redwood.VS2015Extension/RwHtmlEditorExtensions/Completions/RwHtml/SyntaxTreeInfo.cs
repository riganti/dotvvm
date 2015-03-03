using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public class SyntaxTreeInfo
    {
        public SyntaxTree Tree { get; set; }
        public SemanticModel SemanticModel { get; set; }
        public Compilation Compilation { get; set; }
    }
}