using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary> The explicitly registered, closed set of types permitted by a polymorphic viewmodel contract. </summary>
    public sealed class JsonPolymorphismInfo
    {
        public Type BaseType { get; }
        public string TypeDiscriminatorPropertyName { get; }
        public IReadOnlyDictionary<Type, object?> DerivedTypes { get; }
        internal IReadOnlyCollection<Type> RegisteredTypes { get; }

        private JsonPolymorphismInfo(Type type, string discriminatorName, Dictionary<Type, object?> derivedTypes, Type[] registeredTypes)
        {
            BaseType = type;
            TypeDiscriminatorPropertyName = discriminatorName;
            DerivedTypes = derivedTypes;
            RegisteredTypes = registeredTypes;
        }

        internal static bool IsPolymorphic(Type type) =>
            type.IsDefined(typeof(JsonPolymorphicAttribute), false) ||
            type.IsDefined(typeof(JsonDerivedTypeAttribute), false);

        internal static JsonPolymorphismInfo? Create(Type type)
        {
            if (!IsPolymorphic(type))
                return null;
            var name = type.GetCustomAttribute<JsonPolymorphicAttribute>(false)?.TypeDiscriminatorPropertyName ?? "$type";
            if (string.IsNullOrEmpty(name))
                throw new NotSupportedException($"The type discriminator name for {type.ToCode()} must not be empty.");
            var result = new Dictionary<Type, object?>();
            var visited = new HashSet<Type>();
            var discriminators = new Dictionary<object, Type>();
            Visit(type);
            result.Remove(type);
            return new JsonPolymorphismInfo(type, name, result, visited.Where(t => t != type).ToArray());

            void Visit(Type current)
            {
                if (!visited.Add(current))
                    return;
                if (current.GetCustomAttribute<DotvvmSerializationAttribute>()?.AllowsDynamicDispatch(false) == true)
                    throw new NotSupportedException($"JsonDerivedType polymorphism on {current.ToCode()} cannot be combined with AllowDynamicDispatch.");
                foreach (var registration in current.GetCustomAttributes<JsonDerivedTypeAttribute>(false))
                {
                    var derived = registration.DerivedType;
                    if (!current.IsAssignableFrom(derived) || derived.ContainsGenericParameters)
                        throw new NotSupportedException($"Registered derived type {derived.ToCode()} must be assignable to {current.ToCode()} and must be closed.");
                    if (name == "$type" && registration.TypeDiscriminator is not null)
                        throw new NotSupportedException($"JsonDerivedType discriminators on {type.ToCode()} require a nondefault TypeDiscriminatorPropertyName. DotVVM reserves $type for hashed type identifiers.");
                    if (!derived.IsAbstract && !derived.IsInterface)
                    {
                        if (result.TryGetValue(derived, out var previous) && !Equals(previous, registration.TypeDiscriminator))
                            throw new NotSupportedException($"Conflicting discriminators for {derived.ToCode()}.");
                        result[derived] = registration.TypeDiscriminator;
                        if (registration.TypeDiscriminator is {} discriminator)
                        {
                            if (discriminators.TryGetValue(discriminator, out var other) && other != derived)
                                throw new NotSupportedException($"Duplicate polymorphic discriminator on {type.ToCode()}.");
                            discriminators[discriminator] = derived;
                        }
                    }
                    Visit(derived);
                }
            }
        }

        /// <summary> Resolve a hashed DotVVM type identifier, without consulting the global serialization cache. </summary>
        public Type ResolveType(string typeId)
        {
            if (!BaseType.IsAbstract && !BaseType.IsInterface && BaseType.GetTypeHash() == typeId)
                return BaseType;
            foreach (var type in DerivedTypes.Keys)
                if (type.GetTypeHash() == typeId)
                    return type;
            throw new JsonException($"Type '{typeId}' is not a registered concrete subtype of {BaseType.ToCode()}.");
        }

        internal void ValidateMaps(IViewModelSerializationMapper mapper)
        {
            foreach (var type in RegisteredTypes.Prepend(BaseType))
            {
                var map = mapper.GetMap(type);
                if (map.Properties.Any(p => p.Name == "$type" ||
                    (TypeDiscriminatorPropertyName != "$type" && p.Name == TypeDiscriminatorPropertyName)))
                    throw new NotSupportedException($"Polymorphic discriminator collides with a serialized member on {type.ToCode()}.");
            }
        }
    }
}
