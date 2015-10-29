using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel
{
    public class ValidationSettingsAttribute: Attribute
    {
        public bool OnlyInTarget { get; set; }
        public string[] OnlyInGroups { get; set; }
        public string[] NotInGroups { get; set; }
    }
}
