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
        public PropertyInfo PropertyInfo { get; set; }

        public string Name { get; set; } 

        public List<ClientExtenderInfo> ClientExtenders { get; set; }

        public ProtectMode ViewModelProtection { get; set; }

        public Type Type { get; set; }

        public bool TransferToServer { get; set; }
        public bool TransferToServerOnlyInPath { get; set; }
        public bool TransferToClient => TransferAfterPostback || TransferFirstRequest;
        public bool TransferAfterPostback { get; set; }
        public bool TransferFirstRequest { get; set; }
        public bool Populate { get; set; }

        public List<ViewModelPropertyValidationRule> ValidationRules { get; set; }

        public IEnumerable<ViewModelPropertyValidationRule> ClientValidationRules
        {
            get { return ValidationRules.Where(r => !string.IsNullOrEmpty(r.ClientRuleName)); }
        }

        public JsonConverter JsonConverter { get; set; }

        /// <summary>
        /// Gets whether the property is transfered both ways.
        /// </summary>
        public bool IsFullyTransfered()
        {
            return TransferToServer && TransferToClient;
        }

        public override string ToString()
        {
            return $"{nameof(ViewModelPropertyMap)}: {Type.Name} {Name}";
        }
    }
}
