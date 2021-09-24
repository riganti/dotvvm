using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Diagnostics
{
    public class ConfigurationPageViewModel : DotvvmViewModelBase
    {
        public int ActiveTab { get; set; } = 0;

        public Section RootSection { get; set; } = new();

        public override Task Load()
        {
            RootSection = GetSection("Root", Context.Configuration);
            return base.Load();
        }

        private static string GetSettingString<TSetting>(TSetting setting)
        {
            if (setting is null)
            {
                return "null";
            }

            if (setting is string
                || typeof(TSetting).IsPrimitive
                || typeof(TSetting).GetMethod(
                    nameof(ToString),
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) is not null)
            {
                return setting.ToString();
            }

            throw new NotImplementedException();
        }

        private static Section GetSection<TConfig>(string name, TConfig config)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<Section> GetSubsections<TConfig>(TConfig config)
        {
            return typeof(TConfig).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.Assembly == typeof(TConfig).Assembly
                    || p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable<>)))
                .Select(p => GetSection(p.Name, p.GetValue(config)));
        }

        public class Section
        {
            public string? Name { get; set; }

            public List<(string key, string value)> Settings { get; set; } = new List<(string key, string value)>();

            public List<Section> Subsections { get; set; } = new List<Section>();
        }
    }
}
