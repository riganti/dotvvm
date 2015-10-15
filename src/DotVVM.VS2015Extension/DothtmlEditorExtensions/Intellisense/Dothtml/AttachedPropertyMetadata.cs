using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml
{
    public class AttachedPropertyMetadata
    {
        public string Name { get; set; }

        public ITypeSymbol Type { get; set; }
    }
}