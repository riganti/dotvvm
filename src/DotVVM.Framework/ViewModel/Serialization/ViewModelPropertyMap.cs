using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.ViewModel.Validation;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class ViewModelPropertyMap
    {
        public ViewModelPropertyMap()
        {
            this.ClientExtenders = new List<ClientExtenderInfo>();
        }

        public PropertyInfo PropertyInfo { get; set; }

        public string Name { get; set; } 

        public List<ClientExtenderInfo> ClientExtenders { get; private set; }

        public ProtectMode ViewModelProtection { get; set; }

        public Type Type { get; set; }

        public bool TransferToServer { get; set; }
        public bool TransferToServerOnlyInPath { get; set; }
        public bool TransferToClient => TransferAfterPostback || TransferFirstRequest;
        public bool TransferAfterPostback { get; set; }
        public bool TransferFirstRequest { get; set; }
        public bool Populate { get; set; }

        public List<IViewModelPropertyValidationRule> ValidationRules { get; set; }

        public IEnumerable<IViewModelPropertyValidationRule> ClientValidationRules
        {
            get { return ValidationRules.Where(r => !string.IsNullOrEmpty(r.ClientRuleName)); }
        }

        public JsonConverter JsonConverter { get; set; }
    }
}
