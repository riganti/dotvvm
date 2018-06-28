using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class EnumSerializationWithJsonConverterViewModel : DotvvmViewModelBase
    {

        // NSwag generates this attribute. It should be ignored by DotVVM.
        [JsonConverter(typeof(StringEnumConverter))]
        public EnumWithJsonConverter EnumValue { get; set; } = EnumWithJsonConverter.One;

        public string Success { get; set; }

        public void Test()
        {
            Success = "Success!";
        }
    }

    public enum EnumWithJsonConverter
    {
        One,
        Two,
        Three
    }
}

