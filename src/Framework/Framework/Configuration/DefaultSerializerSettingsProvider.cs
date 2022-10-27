using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Framework.Configuration
{
    public sealed class DefaultSerializerSettingsProvider
    {
        private const int defaultMaxSerializationDepth = 64;
        internal readonly JsonSerializerSettings Settings;

        public JsonSerializerSettings GetSettingsCopy()
        {
            return CreateSettings();
        }

        private JsonSerializerSettings CreateSettings()
        {
            var converters = new List<JsonConverter>
            {
                new DotvvmDateTimeConverter(),
                new DotvvmDateOnlyConverter(),
                new DotvvmTimeOnlyConverter(),
                new StringEnumConverter(),
                new DotvvmDictionaryConverter(),
                new DotvvmByteArrayConverter()
            };
            converters.AddRange(ReflectionUtils.GetCustomPrimitiveTypeJsonConverters());

            return new JsonSerializerSettings()
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                Converters = converters,
                MaxDepth = defaultMaxSerializationDepth
            };
        }

        public static DefaultSerializerSettingsProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLocker)
                    {
                        if (instance == null)
                        {
                            instance = new DefaultSerializerSettingsProvider();
                        }
                    }
                }
                return instance;
            }
        }
        private static DefaultSerializerSettingsProvider? instance;
        private static object instanceLocker = new();

        private DefaultSerializerSettingsProvider()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { MaxDepth = defaultMaxSerializationDepth };
            Settings = CreateSettings();
        }
    }
}
