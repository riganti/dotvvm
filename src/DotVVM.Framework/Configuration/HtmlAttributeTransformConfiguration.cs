using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using DotVVM.Framework.Controls;
using System.Reflection;

namespace DotVVM.Framework.Configuration
{
    public sealed class HtmlAttributeTransformConfiguration
    {
        private Lazy<IHtmlAttributeTransformer> instance;


        [JsonProperty("type")]
        public Type Type
        {
            get => _type;
            set { ThrowIfFrozen(); _type = value; }
        }
        private Type _type;

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtensionData
        {
            get => _extensionData;
            set { ThrowIfFrozen(); _extensionData = value; }
        }
        private IDictionary<string, JToken> _extensionData;


        public HtmlAttributeTransformConfiguration()
        {
            instance = new Lazy<IHtmlAttributeTransformer>(CreateInstance, true);
        }

        public IHtmlAttributeTransformer GetInstance()
        {
            if (isFrozen)
                return instance.Value;
            else
                throw new NotSupportedException("Can not use HtmlAttributeTransformConfiguration since it can still be modified");
        }



        private IHtmlAttributeTransformer CreateInstance()
        {
            var transformer = (IHtmlAttributeTransformer)Activator.CreateInstance(Type);

            // apply extension attributes
            if (ExtensionData != null)
            {
                foreach (var extension in ExtensionData)
                {
                    var prop = Type.GetProperty(extension.Key);
                    prop.SetValue(transformer, extension.Value.ToObject(prop.PropertyType));
                }
            }

            return transformer;
        }

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw new InvalidOperationException("The HtmlAttributeTransformConfiguration is frozen and can be no longer modified.");
        }
        public void Freeze()
        {
            this.isFrozen = true;
            FreezableDictionary.Freeze(ref this._extensionData);
            // unfortunately, the stored JTokens are still mutable :(
            // it may get solved at some point, https://github.com/JamesNK/Newtonsoft.Json/issues/468
        }
    }
}
