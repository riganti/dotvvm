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
        public JsonSerializerSettings Settings { get; private set; }

        public JsonSerializerSettings GetSettingsCopy()
        {
            var clone = JsonConvert.SerializeObject(Settings, Settings);
            return JsonConvert.DeserializeObject<JsonSerializerSettings>(clone, Settings);
        }

        public static DefaultSerializerSettingsProvider Instance {
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
            Settings = new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            };
            Settings.Converters.Add(new DotvvmDateTimeConverter());
            Settings.Converters.Add(new StringEnumConverter());
        }
    }
}
