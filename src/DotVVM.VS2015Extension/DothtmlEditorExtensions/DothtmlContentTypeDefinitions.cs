using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Utilities;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions
{

    public class DothtmlContentTypeDefinitions
    {
        public const string DothtmlContentType = "dothtml";


        [Export(typeof(ContentTypeDefinition))]
        [Name(DothtmlContentType)]
        [BaseDefinition("htmlx")]
        public ContentTypeDefinition DothtmlContentTypeDefinition { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [FileExtension(".dothtml")]
        [ContentType(DothtmlContentType)]
        public FileExtensionToContentTypeDefinition DothtmlFileExtensionDefinition { get; set; }
         
    }

}
