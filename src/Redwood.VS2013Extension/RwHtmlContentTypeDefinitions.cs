using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Redwood.VS2013Extension
{

    public class RwHtmlContentTypeDefinitions
    {
        public const string RwHtmlContentType = "rwhtml";


        [Export(typeof(ContentTypeDefinition))]
        [Name(RwHtmlContentType)]
        [BaseDefinition("htmlx")]
        public ContentTypeDefinition RwHtmlContentTypeDefinition { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [FileExtension(".rwhtml")]
        [ContentType(RwHtmlContentType)]
        public FileExtensionToContentTypeDefinition RwHtmlFileExtensionDefinition { get; set; }
         
    }

}
