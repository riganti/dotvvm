using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary>
    /// A converter for collections that supports population of existing instances.
    /// This allows preserving collection references during postback updates.
    /// </summary>
    public class DotvvmCollectionConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            IsCollection(typeToConvert) && !ReflectionUtils.IsPrimitiveType(ReflectionUtils.GetEnumerableType(typeToConvert) ?? typeof(object));

        static bool IsAbstractType(Type type) =>
            type.IsAbstract || type == typeof(object);

        static bool IsCollection(Type type)
        {
            if (type.IsArray && type.GetArrayRank() == 1)
                return true;

            if (!type.IsGenericType)
                return false;

            var genericTypeDef = type.GetGenericTypeDefinition();
            return genericTypeDef == typeof(List<>) ||
                   genericTypeDef == typeof(ImmutableArray<>) ||
                   genericTypeDef == typeof(ImmutableList<>) ||
                   genericTypeDef == typeof(IList<>) ||
                   genericTypeDef == typeof(ICollection<>) ||
                   genericTypeDef == typeof(IReadOnlyList<>) ||
                   genericTypeDef == typeof(IReadOnlyCollection<>) ||
                   genericTypeDef == typeof(IEnumerable<>);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type elementType;
            Type converterType;

            if (typeToConvert.IsArray)
            {
                elementType = typeToConvert.GetElementType()!;
                converterType = typeof(ArrayConverter<>).MakeGenericType(elementType);
            }
            else if (typeToConvert.IsGenericType)
            {
                var genericTypeDef = typeToConvert.GetGenericTypeDefinition();
                var genericArgs = typeToConvert.GetGenericArguments();

                if (genericTypeDef == typeof(List<>) ||
                    genericTypeDef == typeof(IList<>) ||
                    genericTypeDef == typeof(ICollection<>))
                {
                    elementType = genericArgs[0];
                    converterType = typeof(ListConverter<,>).MakeGenericType(elementType, typeToConvert);
                }
                else if (genericTypeDef == typeof(IReadOnlyList<>) ||
                         genericTypeDef == typeof(IReadOnlyCollection<>) ||
                         genericTypeDef == typeof(IEnumerable<>))
                {
                    elementType = genericArgs[0];
                    converterType = typeof(ReadonlyListCollection<,>).MakeGenericType(elementType, typeToConvert);
                }
                else if (genericTypeDef == typeof(ImmutableArray<>))
                {
                    elementType = genericArgs[0];
                    converterType = typeof(ImmutableArrayConverter<>).MakeGenericType(elementType);
                }
                else if (genericTypeDef == typeof(ImmutableList<>))
                {
                    elementType = genericArgs[0];
                    converterType = typeof(ImmutableListConverter<>).MakeGenericType(elementType);
                }
                else
                    return null;
            }
            else
                return null;

            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }

        private abstract class CollectionConverterBase<TElement, TCollection> : JsonConverter<TCollection>, IDotvvmJsonConverter<TCollection>
            where TCollection : IEnumerable<TElement>
        {
            private JsonConverter? defaultElementConverter;
            private readonly bool polymorphic = IsAbstractType(typeof(TElement));

            public override TCollection? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                this.Read(ref reader, typeToConvert, options, DotvvmSerializationState.Current!);

            public TCollection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return default!;

                reader.AssertRead(JsonTokenType.StartArray);
                var elements = new List<TElement>();

                if (reader.TokenType == JsonTokenType.EndArray)
                    return CreateCollection(elements, default);

                var converter = GetDefaultConverter(options);

                if (converter is IDotvvmJsonConverter<TElement> dotvvmConverter)
                {
                    do
                    {
                        var item = dotvvmConverter.Read(ref reader, typeof(TElement), options, state)!;
                        elements.Add(item);
                        reader.AssertRead();
                    }
                    while (reader.TokenType != JsonTokenType.EndArray);
                }
                else if (converter is IDotvvmJsonConverter dotvvmUntypedConverter)
                {
                    do
                    {
                        var item = (TElement)dotvvmUntypedConverter.ReadUntyped(ref reader, typeof(TElement), options, state)!;
                        elements.Add(item);
                        reader.AssertRead();
                    }
                    while (reader.TokenType != JsonTokenType.EndArray);
                }
                else
                {
                    do
                    {
                        var item = JsonSerializer.Deserialize<TElement>(ref reader, options)!;
                        elements.Add(item);
                        reader.AssertRead();
                    }
                    while (reader.TokenType != JsonTokenType.EndArray);
                }
                
                return CreateCollection(elements, default);
            }

            private JsonConverter GetDefaultConverter(JsonSerializerOptions options)
            {
                if (defaultElementConverter is null)
                {
                    var conv = options.GetConverter(typeof(TElement));
                    Interlocked.CompareExchange(ref defaultElementConverter, conv, null);
                }
                return defaultElementConverter;
            }

            public TCollection Populate(ref Utf8JsonReader reader, Type typeToConvert, TCollection existingValue, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return default!;

                var polymorphic = this.polymorphic && !typeof(TElement).IsValueType;

                if (existingValue is null ||
                    existingValue is ICollection { Count: 0 } ||
                    !polymorphic && GetDefaultConverter(options) is not IDotvvmJsonConverter)
                    return Read(ref reader, typeToConvert, options, state);

                reader.AssertRead(JsonTokenType.StartArray);

                var elements = new List<TElement>();
                if (reader.TokenType == JsonTokenType.EndArray)
                    return CreateCollection(elements, existingValue);

                var existingItems = existingValue.ToArray();

                if (polymorphic)
                    PopulateCorePolymorphic(ref reader, options, state, elements, existingItems);
                else
                {
                    var converter = (IDotvvmJsonConverter)GetDefaultConverter(options).NotNull();
                    PopulateCoreDotvvmConverter(ref reader, options, state, elements, existingItems, converter);
                }

                return CreateCollection(elements, existingValue);
            }

            private void PopulateCoreDotvvmConverter(ref Utf8JsonReader reader, JsonSerializerOptions options, DotvvmSerializationState state, List<TElement> elements, TElement[] existingItems, IDotvvmJsonConverter converter)
            {
                int index = 0;
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    TElement? item;
                    if (reader.TokenType == JsonTokenType.Null && typeof(TElement).IsAssignableFromNull())
                        item = default;
                    else if (index < existingItems.Length && existingItems[index] is { } existingItem)
                        item = (TElement?)converter.PopulateUntyped(ref reader, typeof(TElement), existingItem, options, state);
                    else
                        item = (TElement?)converter.ReadUntyped(ref reader, typeof(TElement), options, state)!;
                    elements.Add(item!);
                    index++;
                    reader.AssertRead();
                }
            }

            private void PopulateCorePolymorphic(ref Utf8JsonReader reader, JsonSerializerOptions options, DotvvmSerializationState state, List<TElement> elements, TElement[] existingItems)
            {
                (Type? lastType, JsonConverter? lastConverter) = (null, null);
                int index = 0;

                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    TElement? item;
                    if (typeof(TElement).IsAssignableFromNull() && reader.TokenType == JsonTokenType.Null)
                    {
                        item = default;
                    }
                    else if (index < existingItems.Length && existingItems[index] is { } existingItem)
                    {
                        JsonConverter elementConverter;
                        if (typeof(TElement).IsValueType || !polymorphic || existingItem.GetType() == typeof(TElement))
                            elementConverter = GetDefaultConverter(options);
                        else if (lastType == existingItem.GetType())
                            elementConverter = lastConverter!;
                        else
                        {
                            lastType = existingItem.GetType();
                            elementConverter = options.GetConverter(lastType);
                            lastConverter = elementConverter;
                        }

                        item = PopulateExistingItem(ref reader, existingItem, elementConverter, options, state);
                    }
                    else
                    {
                        var elementConverter = GetDefaultConverter(options);
                        if (elementConverter is IDotvvmJsonConverter<TElement> dotvvmElementConverter)
                            item = dotvvmElementConverter.Read(ref reader, typeof(TElement), options, state)!;
                        else
                            item = JsonSerializer.Deserialize<TElement>(ref reader, options)!;
                    }
                    elements.Add(item!);
                    index++;
                    reader.AssertRead();
                }
            }

            private TElement PopulateExistingItem(ref Utf8JsonReader reader, [DisallowNull] TElement existingItem, JsonConverter converter, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if ((polymorphic || existingItem.GetType() != typeof(TElement)) && converter is IDotvvmJsonConverter dotvvmConverter)
                {
                    return (TElement)dotvvmConverter.PopulateUntyped(ref reader, existingItem.GetType(), existingItem, options, state)!;
                }
                else
                {
                    var staticConverter = converter as JsonConverter<TElement> ?? (JsonConverter<TElement>)options.GetConverter(typeof(TElement));
                    if (staticConverter is IDotvvmJsonConverter<TElement> dotvvmStaticConverter)
                    {
                        return dotvvmStaticConverter.Populate(ref reader, typeof(TElement), existingItem, options, state);
                    }
                    else
                    {
                        // fallback to deserialization for non-DotVVM converters
                        return (TElement)JsonSerializer.Deserialize(ref reader, existingItem.GetType(), options)!;
                    }
                }
            }

            public override void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options) =>
                this.Write(writer, value, options, DotvvmSerializationState.Current!);

            public void Write(Utf8JsonWriter writer, TCollection value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true)
            {
                writer.WriteStartArray();
                (Type? lastType, JsonConverter? lastConverter) = (null, null);
                foreach (var item in value)
                {
                    if (item == null)
                    {
                        writer.WriteNullValue();
                    }
                    else
                    {
                        var itemType = item.GetType();

                        if (!typeof(TElement).IsValueType && polymorphic && itemType != typeof(TElement))
                        {
                            JsonSerializer.Serialize(writer, item, itemType, options);
                        }
                        else if (GetDefaultConverter(options) is IDotvvmJsonConverter dotvvmConverter)
                        {
                            dotvvmConverter.WriteUntyped(writer, item, options, state, requireTypeField || itemType != typeof(TElement), true);
                        }
                        else
                        {
                            JsonSerializer.Serialize(writer, item, itemType, options);
                        }
                    }
                }
                writer.WriteEndArray();
            }

            public object? ReadUntyped(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options, DotvvmSerializationState state) =>
                this.Read(ref reader, typeToConvert, options, state);

            public object? PopulateUntyped(ref Utf8JsonReader reader, Type typeToConvert, object? value, JsonSerializerOptions options, DotvvmSerializationState state)
            {
                if (value == null)
                    return this.Read(ref reader, typeToConvert, options, state);
                return this.Populate(ref reader, typeToConvert, (TCollection)value, options, state);
            }

            public void WriteUntyped(Utf8JsonWriter writer, object? value, JsonSerializerOptions options, DotvvmSerializationState state, bool requireTypeField = true, bool wrapObject = true) =>
                this.Write(writer, (TCollection)value!, options, state, requireTypeField, wrapObject);

            /// <summary>
            /// Creates a collection from the populated elements.
            /// </summary>
            /// <param name="elements">The list of elements to include in the collection</param>
            /// <param name="oldCollection">The existing collection (may be null)</param>
            /// <returns>The new or updated collection</returns>
            protected abstract TCollection CreateCollection(List<TElement> elements, TCollection? oldCollection);
        }

        private class ArrayConverter<T> : CollectionConverterBase<T, T[]>
        {
            protected override T[] CreateCollection(List<T> elements, T[]? oldCollection) =>
                elements.ToArray();
        }

        private class ListConverter<TElement, TCollection> : CollectionConverterBase<TElement, TCollection>
            where TCollection : ICollection<TElement>
        {
            protected override TCollection CreateCollection(List<TElement> elements, TCollection? oldCollection)
            {
                TCollection collection;
                
                if (oldCollection != null)
                {
                    oldCollection.Clear();
                    collection = oldCollection;
                }
                else
                {
                    var collectionType = typeof(TCollection);
                    if (collectionType.IsInterface || typeof(TCollection) == typeof(List<TElement>))
                        return (TCollection)(ICollection<TElement>)elements;
                    else
                        collection = (TCollection)Activator.CreateInstance(collectionType)!;
                }

                foreach (var element in elements)
                    collection.Add(element);

                return collection;
            }
        }

        private class ReadonlyListCollection<TElement, TCollection> : CollectionConverterBase<TElement, TCollection>
            where TCollection : IEnumerable<TElement>
        {
            protected override TCollection CreateCollection(List<TElement> elements, TCollection? oldCollection) =>
                oldCollection is {} && elements.SequenceEqual(oldCollection) ? oldCollection :
                (TCollection)(IEnumerable<TElement>)elements.ToArray();
        }

        private class ImmutableArrayConverter<T> : CollectionConverterBase<T, ImmutableArray<T>>
        {
            protected override ImmutableArray<T> CreateCollection(List<T> elements, ImmutableArray<T> oldCollection) =>
                !oldCollection.IsDefault && elements.SequenceEqual(oldCollection) ? oldCollection :
                elements.ToImmutableArray();
        }

        private class ImmutableListConverter<T> : CollectionConverterBase<T, ImmutableList<T>>
        {
            protected override ImmutableList<T> CreateCollection(List<T> elements, ImmutableList<T>? oldCollection) =>
                oldCollection is {} && elements.SequenceEqual(oldCollection) ? oldCollection :
                elements.ToImmutableList();
        }
    }
}
