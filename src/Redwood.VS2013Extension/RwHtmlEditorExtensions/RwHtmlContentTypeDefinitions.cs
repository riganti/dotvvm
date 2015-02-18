using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Utilities;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions
{

    public class RwHtmlContentTypeDefinitions
    {
        public const string RwHtmlContentType = "rwhtml";


        [Export(typeof(ContentTypeDefinition))]
        [Name(RwHtmlContentType)]
        [BaseDefinition("text")]
        public ContentTypeDefinition RwHtmlContentTypeDefinition { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [FileExtension(".rwhtml")]
        [ContentType(RwHtmlContentType)]
        public FileExtensionToContentTypeDefinition RwHtmlFileExtensionDefinition { get; set; }
         
    }

}
