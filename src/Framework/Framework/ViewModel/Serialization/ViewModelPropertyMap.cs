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

        /// <summary> Property name, as seen in the serialized JSON and client-side. Note that it will be different than `PropertyInfo.Name`, if `[Bind(Name = X)]` or `[JsonProperty(X)]` is used. </summary>
        public string Name { get; set; } 

        /// <summary> Client extenders which will be applied to the created knockout observable. </summary>
        public List<ClientExtenderInfo> ClientExtenders { get; } = new();

        public ProtectMode ViewModelProtection { get; set; }

        public Type Type { get; set; }

        public Direction BindDirection { get; set; } = Direction.None;

        public bool TransferToServer { get; set; }
        public bool TransferToServerOnlyInPath { get; set; }
        public bool TransferToClient => TransferAfterPostback || TransferFirstRequest;
        public bool TransferAfterPostback { get; set; }
        public bool TransferFirstRequest { get; set; }
        /// <summary> When true, an existing object in this property will be preserved during deserialization. A new object will only be created if the property is null, or if we need to call the constructor to set some properties. </summary>
        public bool Populate { get; set; }

        /// <summary> List of validation rules (~= validation attributes) on this property. Includes rules which can't be run client-side </summary>
        public List<ViewModelPropertyValidationRule> ValidationRules { get; } = new();

        public IEnumerable<ViewModelPropertyValidationRule> ClientValidationRules
        {
            get { return ValidationRules.Where(r => !string.IsNullOrEmpty(r.ClientRuleName)); }
        }

        public JsonConverter? JsonConverter { get; set; }

        /// <summary> Used if the class doesn't have parameter-less constructor. This is the parameter where this property value should be when calling the constructor. Property setter will not be called, even if present </summary>
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
