using System.Linq;
using System;
using System.Text.Json;
using System.Diagnostics;
using System.Buffers;
using DotVVM.Framework.ViewModel.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotVVM.Framework.Utils;
public ref struct JsonDiffWriter
{
    public static void ComputeDiff(Utf8JsonWriter writer, JsonElement sourceElement, ReadOnlySpan<byte> targetJsonUtf8)
    {
        if (sourceElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Root value must be an object", nameof(sourceElement));

        Span<byte> lazyStack = stackalloc byte[1024];
        Span<int> indexBuffer = stackalloc int[32];
        var diffWriter = new JsonDiffWriter(lazyStack, indexBuffer, writer, sourceElement, targetJsonUtf8);
        try
        {
            diffWriter.target.AssertRead();
            if (diffWriter.target.TokenType != JsonTokenType.StartObject)
            {
                writer.WriteRawValue(targetJsonUtf8);
            }
            else
            {
                diffWriter.DiffObject(sourceElement);
            }
        }
        finally
        {
            diffWriter.Dispose();
        }
    }

#if DEBUG
    const bool SkipJsonCopyValidation = false;
#else
    const bool SkipJsonCopyValidation = true;
#endif

    void CopyValue()
    {
        Debug.Assert(target.TokenType != JsonTokenType.PropertyName);

        switch (target.TokenType)
        {
            case JsonTokenType.StartArray or JsonTokenType.StartObject: {
                var startIndex = (int)target.TokenStartIndex;
                target.Skip();

                Debug.Assert(target.TokenType is JsonTokenType.EndArray or JsonTokenType.EndObject);

                var span = targetJsonUtf8.Slice(startIndex, (int)target.TokenStartIndex - startIndex + target.GetValueLength());
                Debug.Assert(span[0] is (byte)'{' or (byte)'[');
                Debug.Assert(span[span.Length - 1] is (byte)'}' or (byte)']');
                writer.WriteRawValue(span, SkipJsonCopyValidation);

                break;
            }
            case JsonTokenType.String: {
                // ValueSpan is exclusive of quotes, and reader.Position includes the quote
                var value = targetJsonUtf8.Slice((int)target.TokenStartIndex, target.GetValueLength() + 2);
                writer.WriteRawValue(value, SkipJsonCopyValidation);
                break;
            }

            default: {
                Debug.Assert(!target.HasValueSequence);
                writer.WriteRawValue(target.ValueSpan, SkipJsonCopyValidation);
                break;
            }
        }
    }

    private Utf8JsonReader target;
    private readonly ReadOnlySpan<byte> targetJsonUtf8;

    private byte[] tempBuffer = [];
    // we need "lazy write" properties to remove empty object properties
    private PropertyStack lazyStack;
    // needed for "lazy writer" of arrays
    private RefList<JsonElement> arrayStack;

    private readonly Utf8JsonWriter writer;

    private JsonDiffWriter(
        Span<byte> nameBuffer,
        Span<int> indexBuffer,
        Utf8JsonWriter writer,
        JsonElement sourceElement,
        ReadOnlySpan<byte> targetJson
    )
    {
        this.lazyStack = new PropertyStack(nameBuffer, indexBuffer);
        this.writer = writer;
        this.arrayStack = new RefList<JsonElement>(8);
        this.targetJsonUtf8 = targetJson;
        this.target = new Utf8JsonReader(targetJson);
    }

    ReadOnlySpan<byte> ReadName(in Utf8JsonReader reader, out bool addedToBuffer)
    {
        Debug.Assert(!reader.HasValueSequence);
        if (!reader.HasValueSequence && !reader.ValueIsEscaped)
        {
            addedToBuffer = false;
            return reader.ValueSpan;
        }
        else
        {
            addedToBuffer = true;
            return lazyStack.AddProperty(in reader);
        }
    }

    static void WriteoutLazyArrayValue(Utf8JsonWriter writer, in JsonElement value)
    {
        var kind = value.ValueKind;
        if (kind == JsonValueKind.Object)
        {
            // '{}' means no changes
            writer.WriteRawValue("{}"u8, SkipJsonCopyValidation);
        }
        else if (kind == JsonValueKind.Array)
        {
            // we have to write the entire array, but if it contains any objects, we can use {} to signify there are no changes
            writer.WriteStartArray();
            foreach (var element in value.EnumerateArray())
                WriteoutLazyArrayValue(writer, element);
            writer.WriteEndArray();
        }
        else
        {
#if NET9_0_OR_GREATER
            // WriteTo actually re-encodes to follow new formatting rules and we don't need that
            writer.WriteRawValue(JsonMarshal.GetRawUtf8Value(value), SkipJsonCopyValidation);
#else
            value.WriteTo(writer);
#endif
        }
    }

    static void WriteoutLazyArray(Utf8JsonWriter writer, int count, in JsonElement arrayJson)
    {
        Debug.Assert(count <= arrayJson.GetArrayLength());
        writer.WriteStartArray();
        int i = 0;
        foreach (var element in arrayJson.EnumerateArray())
        {
            if (i >= count)
                break;

            WriteoutLazyArrayValue(writer, element);

            i++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void WriteoutLazyStack()
    {
        if (IsInLazyMode)
            WriteoutLazyStackCore();
    }

    void WriteoutLazyStackCore()
    {
        int arrayIndex = 0;
        for (int i = 0; i < lazyStack.Count; i++)
        {
            int index = lazyStack.Indexes[i];
            if (index >= 0)
            { // object
                if (i != 0)
                    writer.WriteStartObject();

                var name = lazyStack.GetName(i);
                writer.WritePropertyName(name);
            }
            else
            { // array
                Debug.Assert(i != 0, "lazy stack cannot start with an array index");
                var count = -1 - index;
                WriteoutLazyArray(writer, count, arrayStack[arrayIndex]);
                arrayIndex += 1;
            }
        }
        lazyStack.Clear();
        arrayStack.Clear();
    }

    bool IsInLazyMode => lazyStack.Count > 0;

    bool ComparePrimitiveValues(in JsonElement source)
    {
        var sourceKind = source.ValueKind;
        var targetToken = target.TokenType;
        if (sourceKind == JsonValueKind.String && targetToken == JsonTokenType.String)
        {
            if (!target.HasValueSequence && !target.ValueIsEscaped)
            {
                return source.ValueEquals(target.ValueSpan);
            }
            else
            {
                var valueLengthEncoded = target.GetValueLength();
                if (tempBuffer.Length < valueLengthEncoded)
                {
                    if (tempBuffer.Length > 0) ArrayPool<byte>.Shared.Return(tempBuffer);
                    tempBuffer = ArrayPool<byte>.Shared.Rent(valueLengthEncoded);
                }

                var valueLength = target.CopyString(tempBuffer);
                return source.ValueEquals(tempBuffer.AsSpan(0, valueLength));
            }
        }
        else if (sourceKind == JsonValueKind.Number && targetToken == JsonTokenType.Number)
        {
            // we are serializing for JS, which does everything in Float64
            return source.GetDouble().Equals(target.GetDouble());
        }
        else if (sourceKind == JsonValueKind.True && targetToken == JsonTokenType.True)
        {
            return true;
        }
        else if (sourceKind == JsonValueKind.False && targetToken == JsonTokenType.False)
        {
            return true;
        }
        else if (sourceKind == JsonValueKind.Null && targetToken == JsonTokenType.Null)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    bool DiffObject(in JsonElement source)
    {
        target.AssertToken(JsonTokenType.StartObject);

        target.AssertRead();

        bool objectStarted;
        if (IsInLazyMode)
        {
            objectStarted = false;
        }
        else
        {
            objectStarted = true;
            writer.WriteStartObject();
        }

        while (target.TokenType != JsonTokenType.EndObject)
        {
            target.AssertToken(JsonTokenType.PropertyName);
            var propertyName = ReadName(in target, out var addedToBuffer);
            target.AssertRead();

            void initProperty(scoped ref JsonDiffWriter self, scoped ReadOnlySpan<byte> propertyName)
            {
                if (!objectStarted)
                {
                    objectStarted = true;
                    self.WriteoutLazyStackCore();
                    if (!addedToBuffer)
                    {
                        self.writer.WriteStartObject();
                        self.writer.WritePropertyName(propertyName);
                    }
                }
                else
                {
                    self.writer.WritePropertyName(propertyName);
                }
            }

            if (!source.TryGetProperty(propertyName, out var sourceValue))
            {
                initProperty(ref this, propertyName);
                CopyValue();
            }
            else
            {
                var sourceKind = sourceValue.ValueKind;
                var targetToken = target.TokenType;

                if (sourceKind == JsonValueKind.Object && targetToken == JsonTokenType.StartObject)
                {
                    if (!addedToBuffer)
                        lazyStack.AddProperty(propertyName);
                    objectStarted |= DiffObject(sourceValue);
                    lazyStack.TryPop();
                    target.AssertRead();
                    continue;
                }
                if (sourceKind == JsonValueKind.Array && targetToken == JsonTokenType.StartArray)
                {
                    if (!addedToBuffer)
                        lazyStack.AddProperty(propertyName);
                    objectStarted |= DiffArray(sourceValue);
                    lazyStack.TryPop();
                    target.AssertRead();
                    continue;
                }

                bool isEqual = ComparePrimitiveValues(in sourceValue);
                if (!isEqual)
                {
                    initProperty(ref this, propertyName);
                    CopyValue();
                }
            }

            if (addedToBuffer)
            {
                lazyStack.TryPop();
            }
            target.AssertRead();
        }

        if (objectStarted)
            writer.WriteEndObject();

        return objectStarted;
    }

    bool DiffArray(in JsonElement source)
    {
        Debug.Assert(source.ValueKind == JsonValueKind.Array);


        bool arrayStarted = false;
        if (IsInLazyMode)
        {
            lazyStack.AddArray();
            arrayStack.Add(source);
        }
        else
        {
            arrayStarted = true;
            writer.WriteStartArray();
        }

        target.AssertRead(JsonTokenType.StartArray);

        foreach (var sourceValue in source.EnumerateArray())
        {
            var targetToken = target.TokenType;
            if (targetToken == JsonTokenType.EndArray)
            {
                // array was shortened
                WriteoutLazyStack();
                arrayStarted = true;
                break;
            }
            var sourceKind = sourceValue.ValueKind;
            if (sourceKind == JsonValueKind.Object && targetToken == JsonTokenType.StartObject)
            {
                arrayStarted |= DiffObject(in sourceValue);
            }
            else if (sourceKind == JsonValueKind.Array && targetToken == JsonTokenType.StartArray)
            {
                arrayStarted |= DiffArray(in sourceValue);
            }
            else if (arrayStarted)
            {
                // something was different, we now have to copy all primitive values regardless of them being equal or not
                CopyValue();
            }
            else
            {
                var isEqual = ComparePrimitiveValues(in sourceValue);
                if (!isEqual)
                {
                    WriteoutLazyStackCore();
                    CopyValue();
                    arrayStarted = true;
                }
            }

            if (!arrayStarted)
                lazyStack.IncrementArray();

            target.AssertRead();
        }

        if (target.TokenType != JsonTokenType.EndArray)
        {
            // arrays were not of equal length
            WriteoutLazyStack();
            while (target.TokenType != JsonTokenType.EndArray)
            {
                CopyValue();
                target.AssertRead();
            }
            arrayStarted = true;
        }

        if (arrayStarted)
        {
            writer.WriteEndArray();
        }
        else
        {
            lazyStack.Pop();
            arrayStack.Pop();
        }
        return arrayStarted;
    }

    public void Dispose()
    {
        lazyStack.Dispose();
        arrayStack.Dispose();
        if (tempBuffer.Length > 0)
            ArrayPool<byte>.Shared.Return(tempBuffer);
    }


    ref struct PropertyStack
    {
        private RefList<byte> nameBuffer;
        // negative - array counts
        // positive or 0 - property name index in the buffer
        private RefList<int> indexBuffer;

        public PropertyStack(Span<byte> initialNameBuffer, Span<int> indexBuffer)
        {
            this.nameBuffer = new RefList<byte>(initialNameBuffer);
            this.indexBuffer = new RefList<int>(indexBuffer);
        }

        public int Count => indexBuffer.Count;
        public Span<int> Indexes => indexBuffer.Span;
        public Span<byte> NameBuffer => nameBuffer.Span;

        public ReadOnlySpan<byte> GetName(int index)
        {
            var start = indexBuffer[index];
            Debug.Assert(start >= 0);
            int end = nameBuffer.Count;
            foreach (var nextIndex in indexBuffer.Slice(1 + index))
            {
                // it's very unlikely someone has many nested arrays, so this loop should really have 1 or 2 iterations
                if (nextIndex >= 0)
                {
                    end = nextIndex;
                    break;
                }
            }

            return nameBuffer.Slice(start, end - start);
        }


        public void Pop()
        {
            var index = indexBuffer.Last;
            if (index >= 0)
                nameBuffer.Truncate(index);
            indexBuffer.Pop();
        }

        public void TryPop()
        {
            if (indexBuffer.Count > 0)
            {
                Pop();
            }
        }

        public ReadOnlySpan<byte> AddProperty(in Utf8JsonReader reader)
        {
            Debug.Assert(reader.TokenType == JsonTokenType.PropertyName);
            var index = nameBuffer.Count;
            indexBuffer.Add(index);

            Span<byte> preallocated = nameBuffer.Preallocate(reader.GetValueLength());
            var length = reader.CopyString(preallocated);
            nameBuffer.PreallocateCommit(length);

            return nameBuffer.Span.Slice(index);
        }

        public ReadOnlySpan<byte> AddProperty(scoped ReadOnlySpan<byte> name)
        {
            var index = nameBuffer.Count;
            indexBuffer.Add(index);
            nameBuffer.AddRange(name);
            return nameBuffer.Span.Slice(index);
        }

        public void AddArray()
        {
            indexBuffer.Add(-1);
        }

        public void IncrementArray()
        {
            Debug.Assert(indexBuffer[indexBuffer.Count - 1] < 0, "Cannot increment non-array");
            indexBuffer[indexBuffer.Count - 1] -= 1;
        }

        public void Dispose()
        {
            nameBuffer.Dispose();
            indexBuffer.Dispose();
        }

        public void Clear()
        {
            nameBuffer.Clear();
            indexBuffer.Clear();
        }
    }

    ref struct RefList<T>
    {
        Span<T> buffer;
        T[]? rented;
        int count;

        public RefList(Span<T> initialCapacity)
        {
            this.buffer = initialCapacity;
            this.rented = null;
            this.count = 0;
        }

        public RefList(int initialCapacity)
        {
            this.rented = ArrayPool<T>.Shared.Rent(initialCapacity);
            this.buffer = rented;
            this.count = 0;
        }

        public int Count => count;

        public void Enlarge(int newCapacity)
        {
            if (newCapacity <= this.buffer.Length)
                return;

            var newBuffer = ArrayPool<T>.Shared.Rent(Math.Max(newCapacity, buffer.Length * 2));
            buffer.CopyTo(newBuffer);
            if (rented != null) ArrayPool<T>.Shared.Return(rented);
            rented = newBuffer;
            buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            if (count == buffer.Length)
                Enlarge(count + 8);
            buffer[count] = item;
            count += 1;
        }

        public void AddRange(scoped ReadOnlySpan<T> items)
        {
            var newCount = count + items.Length;
            if (newCount > this.buffer.Length)
                Enlarge(newCount);
            items.CopyTo(this.buffer.Slice(count));
            count = newCount;
        }

        public void Pop()
        {
            Debug.Assert(count >= 1);
            count--;
        }

        public void Clear() => Truncate(0);
        public void Truncate(int length)
        {
            Debug.Assert(length >= 0 && length <= count);
            count = length;
        }

        public T? LastOrDefault() => count > 0 ? buffer[count - 1] : default;

        public ref T this[int index] => ref buffer[index];
        public ref T Last => ref buffer[count - 1];

        public Span<T> Span => buffer.Slice(0, count);

        public Span<T> Slice(int from) => buffer.Slice(from, count - from);
        public Span<T> Slice(int from, int length) => buffer.Slice(from, length);

        public Span<T> Preallocate(int maxSize)
        {
            if (maxSize + this.count > this.buffer.Length)
                Enlarge(this.count + maxSize);
            return buffer.Slice(this.count, maxSize);
        }

        public void PreallocateCommit(int size)
        {
            this.count += size;
            Debug.Assert(this.count <= this.buffer.Length);
        }

        public void Dispose()
        {
            if (this.rented is {} returnBuffer)
            {
                this = default;
                ArrayPool<T>.Shared.Return(returnBuffer,
#if DotNetCore          // make sure our JsonElements don't pin the document forever
                        clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>()
#else
                        clearArray: true
#endif
                        );
            }
        }
    }
}
