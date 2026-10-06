using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        private readonly Dictionary<string, Type> allowedTypes;
        private readonly object? baseDiscriminator;

        private JsonPolymorphismInfo(Type type, string discriminatorName, Dictionary<Type, object?> derivedTypes, Type[] registeredTypes, object? baseDiscriminator)
        {
            BaseType = type;
            TypeDiscriminatorPropertyName = discriminatorName;
            DerivedTypes = derivedTypes;
            RegisteredTypes = registeredTypes;
            this.baseDiscriminator = baseDiscriminator;
            allowedTypes = derivedTypes.Keys.ToDictionary(t => t.GetTypeHash(), StringComparer.Ordinal);
            if (!type.IsAbstract && !type.IsInterface)
                allowedTypes.Add(type.GetTypeHash(), type);
        }

        internal static bool IsPolymorphic(Type type) =>
            type.IsDefined(typeof(JsonPolymorphicAttribute), false) ||
            type.IsDefined(typeof(JsonDerivedTypeAttribute), false);

        internal static IEnumerable<JsonPolymorphismInfo> GetRegisteredContracts(Type type)
        {
            var ancestors = type.GetInterfaces().AsEnumerable();
            for (var ancestor = type.BaseType; ancestor is not null; ancestor = ancestor.BaseType)
                ancestors = ancestors.Append(ancestor);
            foreach (var ancestor in ancestors)
                if (Create(ancestor) is {} contract && contract.DerivedTypes.ContainsKey(type))
                    yield return contract;
        }

        internal static IReadOnlyDictionary<string, object> GetDiscriminators(Type type, IEnumerable<JsonPolymorphismInfo> contracts)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var contract in contracts)
            {
                if (contract.TypeDiscriminatorPropertyName == "$type" || contract.GetDiscriminator(type) is not {} discriminator)
                    continue;
                if (result.TryGetValue(contract.TypeDiscriminatorPropertyName, out var previous) && !Equals(previous, discriminator))
                    throw new NotSupportedException($"Conflicting polymorphic discriminators for {type.ToCode()}.");
                result[contract.TypeDiscriminatorPropertyName] = discriminator;
            }
            return result;
        }

        private static readonly ConcurrentDictionary<Type, bool> protectedContractCache = new();
        internal static void ClearCache() => protectedContractCache.Clear();
        internal static bool ContainsPolymorphism(Type type) =>
            protectedContractCache.GetOrAdd(type, t => Visit(t, new HashSet<Type>()));

        private static bool Visit(Type type, HashSet<Type> visited)
        {
            if (!visited.Add(type)) return false;
            if (ReflectionUtils.IsEnumerable(type))
                return ReflectionUtils.GetEnumerableType(type) is {} elementType && Visit(elementType, visited);
            if (!ViewModelJsonConverter.CanConvertType(type))
                return false;
            if (IsPolymorphic(type) || GetRegisteredContracts(type).Any())
                return true;
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (member.GetCustomAttribute<BindAttribute>()?.Direction == Direction.None ||
                    SerialiationMapperAttributeHelper.IsJsonIgnore(member) ||
                    member.IsDefined(typeof(JsonConverterAttribute)))
                    continue;
                var memberType = member switch {
                    PropertyInfo { GetMethod: not null } property when property.GetIndexParameters().Length == 0 => property.PropertyType,
                    FieldInfo field when field.IsDefined(typeof(BindAttribute)) || field.IsDefined(typeof(JsonIncludeAttribute)) ||
                        (type.IsGenericType && type.FullName!.StartsWith("System.ValueTuple`")) => field.FieldType,
                    _ => null
                };
                if (memberType is not null && Visit(memberType, visited))
                    return true;
            }
            return false;
        }

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
            result.TryGetValue(type, out var baseDiscriminator);
            result.Remove(type);
            return new JsonPolymorphismInfo(type, name, result, visited.Where(t => t != type).ToArray(), baseDiscriminator);

            void Visit(Type current)
            {
                if (!visited.Add(current))
                    return;
                if (current.GetCustomAttribute<DotvvmSerializationAttribute>()?.AllowsDynamicDispatch(false) == true)
                    throw new NotSupportedException($"JsonDerivedType polymorphism on {current.ToCode()} cannot be combined with AllowDynamicDispatch.");
                var registrations = current.GetCustomAttributes<JsonDerivedTypeAttribute>(false).ToArray();
                var currentName = current.GetCustomAttribute<JsonPolymorphicAttribute>(false)?.TypeDiscriminatorPropertyName ?? "$type";
                if (current != type && currentName != name && registrations.Any(r => r.TypeDiscriminator is not null))
                    throw new NotSupportedException($"Nested polymorphic contracts with custom discriminators must use the same TypeDiscriminatorPropertyName. {type.ToCode()} uses '{name}', but {current.ToCode()} uses '{currentName}'.");
                foreach (var registration in registrations)
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
            if (allowedTypes.TryGetValue(typeId, out var type))
                return type;
            throw new JsonException($"Type '{typeId}' is not a registered concrete subtype of {BaseType.ToCode()}.");
        }

        internal object? GetDiscriminator(Type type) =>
            type == BaseType ? baseDiscriminator : DerivedTypes.GetValueOrDefault(type);

        internal void ValidateMaps(IViewModelSerializationMapper mapper, JsonSerializerOptions? options = null)
        {
            foreach (var type in RegisteredTypes.Prepend(BaseType))
            {
                if (!ViewModelJsonConverter.CanConvertType(type) ||
                    (options is not null && options.GetConverter(type) is not IDotvvmJsonConverter))
                    throw new NotSupportedException($"Registered polymorphic type {type.ToCode()} must use the DotVVM converter; custom converters and DisableDotvvmConverter on registered subtypes are not supported.");
                var map = mapper.GetMap(type);
                if (map.Properties.Any(p => p.Name == "$type" ||
                    (TypeDiscriminatorPropertyName != "$type" && p.Name == TypeDiscriminatorPropertyName)))
                    throw new NotSupportedException($"Polymorphic discriminator collides with a serialized member on {type.ToCode()}.");
            }
        }
    }
}
