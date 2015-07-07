using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    public class SyntaxTreeInfo
    {
        public SyntaxTree Tree { get; set; }
        public SemanticModel SemanticModel { get; set; }
        public Compilation Compilation { get; set; }
    }
}