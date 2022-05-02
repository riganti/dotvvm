using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.ViewModel.Validation;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class ViewModelPropertyMap
    {
        public ViewModelPropertyMap(PropertyInfo propertyInfo, string name, ProtectMode viewModelProtection, Type type, bool transferToServer, bool transferAfterPostback, bool transferFirstRequest, bool populate)
        {
            PropertyInfo = propertyInfo;
            Name = name;
            ViewModelProtection = viewModelProtection;
            Type = type;
            TransferToServer = transferToServer;
            TransferAfterPostback = transferAfterPostback;
            TransferFirstRequest = transferFirstRequest;
            Populate = populate;
        }

        public PropertyInfo PropertyInfo { get; set; }

        public string Name { get; set; } 

        public List<ClientExtenderInfo> ClientExtenders { get; } = new();

        public ProtectMode ViewModelProtection { get; set; }

        public Type Type { get; set; }

        public Direction BindDirection { get; set; } = Direction.None;

        public bool TransferToServer { get; set; }
        public bool TransferToServerOnlyInPath { get; set; }
        public bool TransferToClient => TransferAfterPostback || TransferFirstRequest;
        public bool TransferAfterPostback { get; set; }
        public bool TransferFirstRequest { get; set; }
        public bool Populate { get; set; }

        public List<ViewModelPropertyValidationRule> ValidationRules { get; } = new();

        public IEnumerable<ViewModelPropertyValidationRule> ClientValidationRules
        {
            get { return ValidationRules.Where(r => !string.IsNullOrEmpty(r.ClientRuleName)); }
        }

        public JsonConverter? JsonConverter { get; set; }

        public ParameterInfo? ConstructorParameter { get; set; }

        /// <summary>
        /// Gets whether the property is transferred both ways.
        /// </summary>
        public bool IsFullyTransferred()
        {
            return TransferToServer && TransferToClient;
        }

        public override string ToString()
        {
            return $"{nameof(ViewModelPropertyMap)}: {Type.Name} {Name}";
        }
        public void ValidateSettings()
        {
            if (ViewModelProtection != ProtectMode.None && !IsFullyTransferred())
            {
                throw new DotvvmCompilationException($"The property {PropertyInfo.Name} of type {Type} uses the Protect attribute, therefore its Bind Direction must be set to {Direction.Both}.");
            }
        }

        public bool IsAvailableOnClient()
        {
            return (TransferToClient || TransferToServer || TransferToServerOnlyInPath)
                && ViewModelProtection != ProtectMode.EncryptData;
        }
    }
}
