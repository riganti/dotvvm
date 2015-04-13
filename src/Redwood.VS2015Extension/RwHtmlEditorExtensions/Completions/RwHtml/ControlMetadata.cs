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
        public string TagPrefix { get; set; }
        public string TagName { get; set; }

        public ControlPropertyMetadata GetProperty(string name)
        {
            return Properties.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}