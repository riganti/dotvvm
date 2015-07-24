using Microsoft.CodeAnalysis;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    public class AttachedPropertyMetadata
    {

        public string Name { get; set; }

        public ITypeSymbol Type { get; set; }

    }
}