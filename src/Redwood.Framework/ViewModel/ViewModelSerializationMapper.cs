using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Redwood.Framework.ViewModel
{
    public class ViewModelSerializationMapper
    {
        public static ViewModelSerializationMap MapClass(Type type)
        {
            return new ViewModelSerializationMap()
            {
                Properties = GetProperties(type),
                Type = type
            };
        }

        public static IEnumerable<ViewModelPropertyMap> GetProperties(Type type)
        {
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                var map = new ViewModelPropertyMap();
                map.Name = p.Name;
                map.ViewModelProtection = ViewModelProtectionSettings.None;
                map.Type = p.PropertyType;
                map.TransferToClient = p.GetMethod != null;
                map.TransferToServer = p.SetMethod != null;

                var bind = p.GetCustomAttribute<BindAttribute>();
                if(bind != null && bind.Direction != Direction.TwoWay)
                {
                    map.TransferToClient = bind.Direction == Direction.OneWay;
                    map.TransferToServer = bind.Direction == Direction.OneWayToSource;
                }

                var crypto = p.GetCustomAttribute<ViewModelProtectionAttribute>();
                if (crypto != null)
                    map.ViewModelProtection = crypto.Settings;

                yield return map;
            }
        }

        public static bool ContainsSpecialAttribute(MemberInfo m)
        {
            return m.GetCustomAttribute<BindAttribute>() != null;
        }

        public static IEnumerable<Type> GetAllViewModels(Assembly assembly)
        {
            return assembly.GetTypes().Where(t =>
                typeof(IRedwoodViewModel).IsAssignableFrom(t) ||
                t.GetMembers().Any(ContainsSpecialAttribute));
        }

        /// <summary>
        /// scan all loaded assemblies (with redwood reference) for view models
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetAllViewModels()
        {
            var redwoodAssembly = typeof(IRedwoodViewModel).Assembly.FullName;
            // include assemblies with redwood referenced to improve performance
            return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetReferencedAssemblies().Any(r => r.FullName == redwoodAssembly)).SelectMany(GetAllViewModels);
        }

        public static IEnumerable<ViewModelSerializationMap> MapAllViewModels()
        {
            return GetAllViewModels().Select(MapClass);
        }
    }
}
