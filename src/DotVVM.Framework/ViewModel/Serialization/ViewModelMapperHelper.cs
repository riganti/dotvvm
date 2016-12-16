using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public static class ViewModelMapperHelper
    {
        // TODO: tests
        // TODO: docs
        public static IViewModelSerializationMapper GetSerializationMapper(this Configuration.DotvvmConfiguration configuration) => configuration.ServiceLocator.GetService<IViewModelSerializationMapper>();

        public static IViewModelSerializationMapper Map(this IViewModelSerializationMapper mapper, Type type, Action<ViewModelSerializationMap> action)
        {
            var map = mapper.GetMap(type);
            action(map);
            map.ResetFunctions();
            return mapper;
        }

        public static ViewModelPropertyMap Property(this ViewModelSerializationMap map, string name)
        {
            var r = map.Properties.SingleOrDefault(p => p.Name == name);
            if (r == null) throw new InvalidOperationException($"Property '{name}' was not found on '{map.Type}'.");
            return r;
        }

        public static ViewModelPropertyMap Bind(this ViewModelPropertyMap property, Direction direction)
        {
            property.TransferAfterPostback = direction.HasFlag(Direction.ServerToClientPostback);
            property.TransferFirstRequest = direction.HasFlag(Direction.ServerToClientFirstRequest);
            property.TransferToServer = direction.HasFlag(Direction.ClientToServerNotInPostbackPath) || direction.HasFlag(Direction.ClientToServerInPostbackPath);
            property.TransferToServerOnlyInPath = !direction.HasFlag(Direction.ClientToServerNotInPostbackPath) && property.TransferToServer;

            return property;
        }

        public static ViewModelPropertyMap Protect(this ViewModelPropertyMap property, ProtectMode protectMode)
        {
            property.ViewModelProtection = protectMode;
            return property;
        }

        public static void Ignore(this ViewModelPropertyMap property)
        {
            property.Bind(Direction.None);
            property.ValidationRules.Clear();
            property.ClientExtenders.Clear();
        }

        public static ViewModelPropertyMap AddClientExtender(this ViewModelPropertyMap property, ClientExtenderInfo clientExtender)
        {
            property.ClientExtenders.Add(clientExtender);
            return property;
        }

        public static ViewModelPropertyMap SetJsonConverter(this ViewModelPropertyMap property, JsonConverter converter)
        {
            property.JsonConverter = converter;
            return property;
        }
    }
}
