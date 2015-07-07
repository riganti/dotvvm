using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    public class ControlMetadata
    {

        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<ControlPropertyMetadata> Properties { get; set; }
        public string TagPrefix { get; set; }
        public string TagName { get; set; }

        public string FullTagName
        {
            get { return string.IsNullOrEmpty(TagPrefix) ? TagName : (TagPrefix + ":" + TagName); }
        }


        public ControlPropertyMetadata GetProperty(string name)
        {
            return Properties.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}