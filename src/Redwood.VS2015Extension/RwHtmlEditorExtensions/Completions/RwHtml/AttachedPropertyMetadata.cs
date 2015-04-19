using Microsoft.CodeAnalysis;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public class AttachedPropertyMetadata
    {

        public string Name { get; set; }

        public ITypeSymbol Type { get; set; }

    }
}