using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace DotVVM.VS2015Extension.Configuration
{
    public class ContentTypeDefinitions
    {
        public const string DothtmlContentType = "dothtml";
        public const string JavaScriptContentType = "JavaScript";
        public const string CSharpContentType = "cs";

        [Export(typeof(ContentTypeDefinition))]
        [Name(DothtmlContentType)]
        [BaseDefinition("htmlx")]
        public ContentTypeDefinition DothtmlContentTypeDefinition { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [FileExtension(".dothtml")]
        [ContentType(DothtmlContentType)]
        public FileExtensionToContentTypeDefinition DothtmlFileExtensionDefinition { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [FileExtension(".dotmaster")]
        [ContentType(DothtmlContentType)]
        public FileExtensionToContentTypeDefinition DotmasterFileExtensionDefinition { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [FileExtension(".dotcontrol")]
        [ContentType(DothtmlContentType)]
        public FileExtensionToContentTypeDefinition DotcontrolFileExtensionDefinition { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [FileExtension(".cs")]
        [ContentType(CSharpContentType)]
        public FileExtensionToContentTypeDefinition CSharpFileExtensionDefinition { get; set; }
    }
}