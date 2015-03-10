using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public class ControlMetadata
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<ControlPropertyMetadata> Properties { get; set; }
    }
}