using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public static class ViewModelMapperHelper
    {
        // TODO: tests
        // TODO: docs
        public static IViewModelSerializationMapper GetSerializationMapper(this Configuration.DotvvmConfiguration configuration) => configuration.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();

        public static IViewModelSerializationMapper Map(this IViewModelSerializationMapper mapper, Type type, Action<ViewModelSerializationMap> action)
        {
            var map = mapper.GetMap(type);
            action(map);
            map.ResetFunctions();
            return mapper;
        }

        public static void SetConstructor(this ViewModelSerializationMap map, ObjectFactory factory)
        {
            map.SetConstructor(p => factory.Invoke(p, new object[0]));
        }

        public static void AllowDependencyInjection(this ViewModelSerializationMap map)
        {
            map.SetConstructor(ActivatorUtilities.CreateFactory(map.Type, Type.EmptyTypes));
        }

        public static ViewModelPropertyMap Property(this ViewModelSerializationMap map, string name) =>
            map.Properties.SingleOrDefault(p => p.PropertyInfo.Name == name) ??
            throw new InvalidOperationException($"Property '{name}' was not found on '{map.Type}'.");

        public static ViewModelPropertyMap PropertyByClientName(this ViewModelSerializationMap map, string name) =>
            map.Properties.SingleOrDefault(p => p.Name == name) ??
            throw new InvalidOperationException($"Property with client name '{name}' was not found on '{map.Type}'.");

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
