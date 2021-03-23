using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Framework.Configuration
{
    public sealed class DefaultSerializerSettingsProvider
    {
        internal readonly JsonSerializerSettings Settings;

        public JsonSerializerSettings GetSettingsCopy()
        {
            return CreateSettings();
        }

        private JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                Converters = new List<JsonConverter>
                {
                    new DotvvmDateTimeConverter(),
                    new StringEnumConverter(),
                    new DotvvmDictionaryConverter()
                }
            };
        }

        public static DefaultSerializerSettingsProvider Instance
        {
            get
            {
                if (instance == null)
                    instance = new DefaultSerializerSettingsProvider();
                return instance;
            }
        }
        private static DefaultSerializerSettingsProvider instance;

        private DefaultSerializerSettingsProvider()
        {
            Settings = CreateSettings();
        }
    }
}
