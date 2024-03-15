using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel.Serialization
{
    /// <summary> Serializes enums as string, using the <see cref="EnumMemberAttribute" />, as Newtonsoft.Json did </summary>
    public class DotvvmEnumConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsEnum;
        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) =>
            (JsonConverter?)CreateConverterGenericMethod.MakeGenericMethod(typeToConvert).Invoke(this, []);

        static MethodInfo CreateConverterGenericMethod = (MethodInfo)MethodFindingHelper.GetMethodFromExpression(() => default(DotvvmEnumConverter)!.CreateConverter<MethodFindingHelper.Generic.Enum>());
        public JsonConverter<TEnum> CreateConverter<TEnum>() where TEnum : unmanaged, Enum
        {
            // if (!ReflectionUtils.EnumInfo<TEnum>.HasEnumMemberField)
            //     return (JsonConverter<TEnum>)new JsonStringEnumConverter<TEnum>().CreateConverter(typeof(TEnum), options)!;

            var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            var isFlags = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);
            var isSigned = underlyingType == typeof(sbyte) || underlyingType == typeof(short) || underlyingType == typeof(int) || underlyingType == typeof(long);

            var fieldList = new List<(TEnum Value, byte[] Name)>();
            var enumToName = new Dictionary<TEnum, byte[]>();
            foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.Name == "value__")
                    continue;
                var name = field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? field.Name;
                var nameUtf8 = StringUtils.Utf8.GetBytes(name);

                if (isFlags && name.IndexOfAny([',', ' ']) >= 0)
                    throw new NotSupportedException("Flags enum cannot have EnumMemberAttribute with comma or a space.");

                var value = (TEnum)field.GetValue(null)!;
                fieldList.Add((value, nameUtf8));
                if (!enumToName.ContainsKey(value))
                    enumToName.Add(value, nameUtf8);
                else
                {
                    if (nameUtf8.Length < enumToName[value].Length || nameUtf8.AsSpan().SequenceCompareTo(enumToName[value]) < 0)
                        enumToName[value] = nameUtf8;
                }
            }
            var fieldsDedup = enumToName.Select(x => (Value: x.Key, Name: x.Value))
                                        .OrderByDescending(x => ToBits(x.Value))
                                        .ToArray();


            var maxNameLen = fieldList.Max(x => x.Name.Length);
            var nameToEnum = new (TEnum Value, byte[] Name)[maxNameLen + 1][];
            foreach (var field in fieldList.GroupBy(x => x.Name.Length)) // TODO: do we want to allow the client to send the duplicate names?
            {
                var array = field.ToArray();
                Array.Sort(array, (a, b) => a.Name.AsSpan().SequenceCompareTo(b.Name.AsSpan()));
                nameToEnum[field.Key] = array;
            }

            ulong allowedBitMap = 0;
            if (isFlags)
            {
                foreach (var field in fieldsDedup)
                {
                    var bits = ToBits(field.Value);
                    if (bits <= 64)
                        allowedBitMap |= 1UL << (int)bits;
                }
            }
            else
            {
                foreach (var field in fieldsDedup)
                    allowedBitMap |= ToBits(field.Value);
            }

            if (isFlags && isSigned)
                return new InnerConverter<TEnum, True, True>(fieldsDedup, enumToName, nameToEnum, maxNameLen, allowedBitMap);
            if (isFlags && !isSigned)
                return new InnerConverter<TEnum, True, False>(fieldsDedup, enumToName, nameToEnum, maxNameLen, allowedBitMap);
            if (!isFlags && isSigned)
                return new InnerConverter<TEnum, False, True>(fieldsDedup, enumToName, nameToEnum, maxNameLen, allowedBitMap);
            if (!isFlags && !isSigned)
                return new InnerConverter<TEnum, False, False>(fieldsDedup, enumToName, nameToEnum, maxNameLen, allowedBitMap);
            throw new NotSupportedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong ToBits<TEnum>(TEnum value) where TEnum : unmanaged, Enum
        {
            if (Unsafe.SizeOf<TEnum>() == 1)
                return Unsafe.As<TEnum, byte>(ref value);
            if (Unsafe.SizeOf<TEnum>() == 2)
                return Unsafe.As<TEnum, ushort>(ref value);
            if (Unsafe.SizeOf<TEnum>() == 4)
                return Unsafe.As<TEnum, uint>(ref value);
            if (Unsafe.SizeOf<TEnum>() == 8)
                return Unsafe.As<TEnum, ulong>(ref value);
            throw new NotSupportedException();
        }

        class InnerConverter<TEnum, IsFlags, IsSigned>(
            (TEnum Value, byte[] Name)[] fields, // sorted by value (ulong), descending
            Dictionary<TEnum, byte[]> enumToName,
            (TEnum Value, byte[] Name)[]?[] nameToEnum, // grouped by length, sorted by name
            int maxNameLen,
            ulong allowedBitMap // bitmap for first 64 non-flags, or all possible flags combined
        ) : JsonConverter<TEnum>
            where TEnum : unmanaged, Enum
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static TEnum ReadNumber(ref Utf8JsonReader reader)
            {
                if (typeof(IsSigned) == typeof(True))
                {
                    if (Unsafe.SizeOf<TEnum>() == 1)
                    {
                        var num = reader.GetSByte();
                        return Unsafe.As<sbyte, TEnum>(ref num);
                    }
                    if (Unsafe.SizeOf<TEnum>() == 2)
                    {
                        var num = reader.GetInt16();
                        return Unsafe.As<short, TEnum>(ref num);
                    }
                    if (Unsafe.SizeOf<TEnum>() == 4)
                    {
                        var num = reader.GetInt32();
                        return Unsafe.As<int, TEnum>(ref num);
                    }
                    if (Unsafe.SizeOf<TEnum>() == 8)
                    {
                        var num = reader.GetInt64();
                        return Unsafe.As<long, TEnum>(ref num);
                    }
                }
                else
                {
                    if (Unsafe.SizeOf<TEnum>() == 1)
                    {
                        var num = reader.GetByte();
                        return Unsafe.As<byte, TEnum>(ref num);
                    }
                    if (Unsafe.SizeOf<TEnum>() == 2)
                    {
                        var num = reader.GetUInt16();
                        return Unsafe.As<ushort, TEnum>(ref num);
                    }
                    if (Unsafe.SizeOf<TEnum>() == 4)
                    {
                        var num = reader.GetUInt32();
                        return Unsafe.As<uint, TEnum>(ref num);
                    }
                    if (Unsafe.SizeOf<TEnum>() == 8)
                    {
                        var num = reader.GetUInt64();
                        return Unsafe.As<ulong, TEnum>(ref num);
                    }
                }
                throw new NotSupportedException();
            }
            public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    var number = ReadNumber(ref reader);
                    if (typeof(IsFlags) == typeof(True))
                    {
                        if ((ToBits(number) & ~allowedBitMap) > 0)
                            ThrowInvalidEnumValue(number);
                    }
                    else
                    {
                        bool isValid;
                        if (ToBits(number) <= 64)
                            isValid = (allowedBitMap & (1UL << (int)ToBits(number))) != 0;
                        else
                            isValid = enumToName.ContainsKey(number);
                        if (!isValid)
                            ThrowInvalidEnumValue(number);
                    }

                    return number;
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    // TODO: allow numbers in string?
                    if (typeof(IsFlags) == typeof(False))
                    {
                        Span<byte> name = maxNameLen < 512 ? stackalloc byte[maxNameLen + 1] : new byte[maxNameLen + 1];
                        var length = reader.CopyString(name);
                        name = name.Slice(0, length);
                        return FindEnumName(name);
                    }
                    else
                    {
                        var valueLength = reader.HasValueSequence ? (int)reader.ValueSequence.Length : reader.ValueSpan.Length;
                        byte[]? rentedBuffer = null;
                        try
                        {
                            Span<byte> buffer = valueLength < 512 ? stackalloc byte[valueLength] : (rentedBuffer = ArrayPool<byte>.Shared.Rent(valueLength));
                            var bufferLength = reader.CopyString(buffer);
                            buffer = buffer.Slice(0, bufferLength);

                            ulong result = 0;
                            while (true)
                            {
                                buffer = buffer.Slice(buffer[0] == ' ' ? 1 : 0);

                                var nextIndex = MemoryExtensions.IndexOf(buffer, (byte)',');
                                if (nextIndex == 0)
                                    return ThrowInvalidEnumName(buffer);

                                var token = nextIndex < 0 ? buffer : buffer.Slice(0, nextIndex);

                                var value = FindEnumName(token);
                                result |= ToBits(value);

                                if (nextIndex < 0)
                                    break;
                                buffer = buffer.Slice(nextIndex + 1);
                            }

                            return Unsafe.As<ulong, TEnum>(ref result);
                        }
                        finally
                        {
                            if (rentedBuffer is {})
                                ArrayPool<byte>.Shared.Return(rentedBuffer);
                        }
                    }
                }
                else
                    return ThrowInvalidToken(reader.TokenType);
            }

            TEnum FindEnumName(ReadOnlySpan<byte> name)
            {
                if (name.Length > maxNameLen)
                    return ThrowInvalidEnumName(name);
                var fields = nameToEnum[name.Length];
                if (fields is null)
                    return ThrowInvalidEnumName(name);
                return BinSearch(name, fields, 0, fields.Length);

                static TEnum BinSearch(ReadOnlySpan<byte> name, (TEnum Value, byte[] Name)[] fields, int start, int end)
                {
                    TailCall:
                    if (start == end)
                        return ThrowInvalidEnumName(name);

                    var mid = (start + end) / 2;
                    var cmp = name.SequenceCompareTo(fields[mid].Name);
                    if (cmp == 0)
                        return fields[mid].Value;
                    if (cmp < 0)
                    {
                        // return BinSearch(name, fields, start, mid);
                        end = mid;
                        goto TailCall;
                    }
                    else
                    {
                        // return BinSearch(name, fields, mid + 1, end);
                        start = mid + 1;
                        goto TailCall;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowInvalidEnumValue(TEnum value)
            {
                throw new JsonException($"'{value}' is not valid {typeof(TEnum).Name}.");
            }
            [MethodImpl(MethodImplOptions.NoInlining)]
            static TEnum ThrowInvalidEnumName(ReadOnlySpan<byte> token)
            {
                throw new JsonException($"'{StringUtils.Utf8.GetString(token.ToArray())}' is not a member of {typeof(TEnum).Name}.");
            }
            [MethodImpl(MethodImplOptions.NoInlining)]
            static TEnum ThrowInvalidToken(JsonTokenType token)
            {
                throw new JsonException($"'{token}' cannot be parsed as enum.");
            }
            static void WriteAsNumber(Utf8JsonWriter writer, TEnum value)
            {
                if (typeof(IsSigned) == typeof(True))
                {
                    if (Unsafe.SizeOf<TEnum>() == 1)
                        writer.WriteNumberValue(Unsafe.As<TEnum, sbyte>(ref value));
                    else if (Unsafe.SizeOf<TEnum>() == 2)
                        writer.WriteNumberValue(Unsafe.As<TEnum, short>(ref value));
                    else if (Unsafe.SizeOf<TEnum>() == 4)
                        writer.WriteNumberValue(Unsafe.As<TEnum, int>(ref value));
                    else if (Unsafe.SizeOf<TEnum>() == 8)
                        writer.WriteNumberValue(Unsafe.As<TEnum, long>(ref value));
                    else
                        throw new NotSupportedException();
                }
                else
                {
                    if (Unsafe.SizeOf<TEnum>() == 1)
                        writer.WriteNumberValue(Unsafe.As<TEnum, byte>(ref value));
                    else if (Unsafe.SizeOf<TEnum>() == 2)
                        writer.WriteNumberValue(Unsafe.As<TEnum, ushort>(ref value));
                    else if (Unsafe.SizeOf<TEnum>() == 4)
                        writer.WriteNumberValue(Unsafe.As<TEnum, uint>(ref value));
                    else if (Unsafe.SizeOf<TEnum>() == 8)
                        writer.WriteNumberValue(Unsafe.As<TEnum, ulong>(ref value));
                    else
                        throw new NotSupportedException();
                }
            }
            public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
            {
                if (enumToName.TryGetValue(value, out var name))
                {
                    writer.WriteStringValue(name);
                    return;
                }

                if (typeof(IsFlags) == typeof(False))
                {
                    WriteAsNumber(writer, value); // TODO: do we want to allow the numbers?
                }
                else
                {
                    var bits = ToBits(value);
                    if (bits == 0)
                    {
                        // if none field existed, we have used it above, see if (enumToName.TryGetValue...
                        writer.WriteNumberValue(0);
                        return;
                    }

                    Span<byte> buffer = stackalloc byte[512];
                    var bufferPosition = 0;

                    foreach (var flag in fields)
                    {
                        var flagBits = ToBits(flag.Value);
                        if ((flagBits & ~bits) != 0)
                            continue; // this flag contains something else, we can't use it

                        bits &= ~flagBits;

                        if (buffer.Length <= bufferPosition + flag.Name.Length + 2)
                        {   // make sure the output buffer is large enough
                            var newSize = Math.Max(buffer.Length * 2, bufferPosition + flag.Name.Length + 2);
                            var newBuffer = new byte[newSize];
                            buffer.Slice(0, bufferPosition).CopyTo(newBuffer);
                            buffer = newBuffer;
                        }

                        if (bufferPosition > 0)
                        {   // insert ', '
                            buffer[bufferPosition++] = (byte)',';
                            if (writer.Options.Indented)
                                buffer[bufferPosition++] = (byte)' ';
                        }

                        flag.Name.CopyTo(buffer.Slice(bufferPosition));
                        bufferPosition += flag.Name.Length;

                        if (bits == 0)
                            break;
                    }

                    if (bits == 0)
                        writer.WriteStringValue(buffer.Slice(0, bufferPosition));
                    else
                        // it isn't possible to set the required flags using names
                        WriteAsNumber(writer, value);
                }
            }
        }

        struct True { }
        struct False { }

        // struct Comparable
    }
}
