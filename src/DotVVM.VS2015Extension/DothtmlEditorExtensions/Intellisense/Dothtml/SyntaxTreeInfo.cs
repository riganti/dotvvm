using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml
{
    public class SyntaxTreeInfo
    {
        public SyntaxTree Tree { get; set; }
        public SemanticModel SemanticModel { get; set; }
        public Compilation Compilation { get; set; }
    }
}