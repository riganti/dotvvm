using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{
    public class ConfigurationPageViewModel : DotvvmViewModelBase
    {
        public int ActiveTab { get; set; } = 0;

        public List<Section> RootSections { get; set; } = new();

        public override Task Load()
        {
            RootSections = new List<Section> { GetSection(Context.Configuration) };
            return base.Load();
        }

        private static string? GetSettingString(object? setting)
        {
            if (setting is null)
            {
                return null;
            }

            return setting.ToString();
        }

        private static Section GetSection(object config)
        {
            var configType = config.GetType();

            var section = new Section {
                Name = configType.Name
            };

            var props = configType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                if (IsSetting(prop))
                {
                    section.Settings.Add(new Setting {
                        Name = prop.Name,
                        Value = GetSettingString(prop.GetValue(config))
                    });
                }
                else if (IsSubsection(prop))
                {
                    var subsection = prop.GetValue(config);
                    if (subsection is not null)
                    {
                        section.Subsections.Add(GetSection(subsection));
                    }
                }
            }

            if (configType.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                foreach (var subsection in (IEnumerable<object>)config)
                {
                    section.Subsections.Add(GetSection(subsection));
                }
            }
            return section;
        }

        private static bool IsSubsection(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<JsonIgnoreAttribute>() is null
                && prop.GetIndexParameters().Length == 0
                && prop.PropertyType.IsClass
                && prop.PropertyType.Assembly == typeof(DotvvmConfiguration).Assembly;
        }

        private static bool IsSetting(PropertyInfo prop)
        {
            return prop.GetCustomAttribute<JsonIgnoreAttribute>() is null
                && prop.GetIndexParameters().Length == 0
                && (prop.PropertyType.IsPrimitive
                || prop.PropertyType.IsEnum
                || prop.PropertyType == typeof(string));
        }

        [DebuggerDisplay("Section {Name} [{Settings.Count}, {Subsections.Count}]")]
        public class Section
        {
            public string? Name { get; set; }

            public List<Setting> Settings { get; set; } = new();

            public List<Section> Subsections { get; set; } = new();
        }

        [DebuggerDisplay("Setting {Name}: {Value}")]
        public class Setting
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
        }
    }
}
