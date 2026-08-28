using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public static class ViewModelMapperHelper
    {
        // TODO: tests
        // TODO: docs
        /// <summary> Returns the DotVVM serialization mapper which can be used to configure how some viewmodels are serialized </summary>
        public static IViewModelSerializationMapper GetSerializationMapper(this Configuration.DotvvmConfiguration configuration) => configuration.ServiceProvider.GetRequiredService<IViewModelSerializationMapper>();

        /// <summary> Configure serialization mapping for the specified viewmodel <paramref name="type"/> </summary>
        public static IViewModelSerializationMapper Map(this IViewModelSerializationMapper mapper, Type type, Action<ViewModelSerializationMap> action)
        {
            var map = mapper.GetMap(type);
            action(map);
            map.ResetFunctions();
            return mapper;
        }

        /// <summary> Configure serialization mapping for the specified viewmodel <typeparam name="T"/> </summary>
        public static IViewModelSerializationMapper Map<T>(this IViewModelSerializationMapper mapper, Action<ViewModelSerializationMap<T>> action)
        {
            var map = mapper.GetMap<T>();
            action(map);
            map.ResetFunctions();
            return mapper;
        }

        public static void SetConstructor(this ViewModelSerializationMap map, ObjectFactory factory)
        {
            ThrowHelpers.ArgumentNull(factory);
            map.SetConstructorUntyped(p => factory.Invoke(p, []));
        }

        public static void AllowDependencyInjection(this ViewModelSerializationMap map)
        {
            map.SetConstructor(ActivatorUtilities.CreateFactory(map.Type, Type.EmptyTypes));
        }

        /// <summary> Find property of the mapped type by its C# name </summary>
        public static ViewModelPropertyMap Property(this ViewModelSerializationMap map, string name) =>
            map.Properties.SingleOrDefault(p => p.PropertyInfo.Name == name) ??
            throw new InvalidOperationException($"Property '{name}' was not found on '{map.Type}'.");

        /// <summary> Find property of the mapped type by the currently configured serialized name (e.g. via <see cref="BindAttribute.Name" />>) </summary>
        public static ViewModelPropertyMap PropertyByClientName(this ViewModelSerializationMap map, string name) =>
            map.Properties.SingleOrDefault(p => p.Name == name) ??
            throw new InvalidOperationException($"Property with client name '{name}' was not found on '{map.Type}'.");

        /// <summary> Sets the binding direction for this property, equivalent to <see cref="BindAttribute" /> </summary>
        public static ViewModelPropertyMap Bind(this ViewModelPropertyMap property, Direction direction)
        {
            property.TransferAfterPostback = direction.HasFlag(Direction.ServerToClientPostback);
            property.TransferFirstRequest = direction.HasFlag(Direction.ServerToClientFirstRequest) || direction.HasFlag(Direction.ClientToServer);
            property.TransferToServer = direction.HasFlag(Direction.ClientToServerNotInPostbackPath) || direction.HasFlag(Direction.ClientToServerInPostbackPath);
            property.TransferToServerOnlyInPath = !direction.HasFlag(Direction.ClientToServerNotInPostbackPath) && property.TransferToServer;

            return property;
        }

        /// <summary> Sets the protection mode for this property, equivalent to <see cref="ProtectAttribute" /> </summary>
        public static ViewModelPropertyMap Protect(this ViewModelPropertyMap property, ProtectMode protectMode)
        {
            property.ViewModelProtection = protectMode;
            return property;
        }

        /// <summary> Exclude this property from DotVVM serialization </summary>
        public static void Ignore(this ViewModelPropertyMap property)
        {
            property.Bind(Direction.None);
            property.ValidationRules.Clear();
            property.ClientExtenders.Clear();
        }

        /// <summary> Configure client-side knockout extender to use on this property's observable. The extender must be available client-side, the serializer will not request its resource by itself. </summary>
        public static ViewModelPropertyMap AddClientExtender(this ViewModelPropertyMap property, ClientExtenderInfo clientExtender)
        {
            ThrowHelpers.ArgumentNull(clientExtender);
            property.ClientExtenders.Add(clientExtender);
            return property;
        }

        /// <summary> Use the specified System.Text.Json converter for this property. If <c>null</c>, use the default converter. </summary>
        public static ViewModelPropertyMap SetJsonConverter(this ViewModelPropertyMap property, System.Text.Json.Serialization.JsonConverter? converter)
        {
            property.JsonConverter = converter;
            return property;
        }
    }
}
