using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotVVM.Framework.ViewModel
{
    public class ViewModelPropertyMap
    {
        public PropertyInfo PropertyInfo { get; set; }

        public string Name { get; set; } 

        public ViewModelProtectionSettings ViewModelProtection { get; set; }

        public Type Type { get; set; }

        public bool TransferToServer { get; set; }

        public bool TransferToClient { get; set; }

        public List<ViewModelPropertyValidationRule> ValidationRules { get; set; }

        public IEnumerable<ViewModelPropertyValidationRule> ClientValidationRules
        {
            get { return ValidationRules.Where(r => !string.IsNullOrEmpty(r.ClientRuleName)); }
        } 

    }
}
