using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Configuration
{
    public class HtmlAttributeTransformConfiguration
    {
        private Lazy<IHtmlAttributeTransformer> instance;


        [JsonProperty("type")]
        public Type Type { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JToken> ExtensionData { get; set; }


        public HtmlAttributeTransformConfiguration()
        {
            instance = new Lazy<IHtmlAttributeTransformer>(CreateInstance, true);
        }

        public IHtmlAttributeTransformer GetInstance()
        {
            return instance.Value;
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
    }
}