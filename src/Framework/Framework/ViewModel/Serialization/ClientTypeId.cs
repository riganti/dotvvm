using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel.Serialization
{
    [StructLayout(LayoutKind.Sequential)]
    [JsonConverter(typeof(ClientTypeId.JsonConverter))]
    public readonly struct ClientTypeId: IEquatable<ClientTypeId>, IComparable<ClientTypeId>
    {
        public const int HashLength = 12;
        public const int Base64HashLength = 16;

        // note that a, b contents depends on system endianity (not that .NET would support anything big endian)
        readonly ulong a;
        readonly ulong b;

        private ClientTypeId(ulong a, ulong b)
        {
            this.a = a;
            this.b = b | (1UL << 56);
        }

        public bool IsDefault => b == 0;

        public ReadOnlySpan<byte> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ulong, byte>(ref Unsafe.AsRef(in this.a)), HashLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> AsSpan(ref ClientTypeId id) =>
            MemoryMarshal.CreateSpan(ref Unsafe.As<ulong, byte>(ref Unsafe.AsRef(in id.a)), HashLength);

        public static bool TryParse(string text, out ClientTypeId result)
        {
            if (text.Length != Base64HashLength)
            {
                result = default;
                return false;
            }
            result = new(0, 0);
            Span<byte> buffer = AsSpan(ref result);
            if (!Convert.TryFromBase64String(text, buffer, out _))
            {
                result = default;
                return false;
            }

            return true;
        }

        public static bool TryParse(ReadOnlySpan<byte> text, out ClientTypeId result)
        {
            if (text.Length != Base64HashLength)
            {
                result = default;
                return false;
            }

            result = new(0, 0);
            Span<byte> buffer = AsSpan(ref result);
            if (System.Buffers.Text.Base64.DecodeFromUtf8(text, buffer, out _, out _, isFinalBlock: true) != System.Buffers.OperationStatus.Done)
            {
                result = default;
                return false;
            }

            return true;
        }


        public void WriteJson(Utf8JsonWriter writer)
        {
            if (IsDefault) writer.WriteNullValue();
            writer.WriteBase64StringValue(Span);
        }
        public void WriteJson(Utf8JsonWriter writer, ReadOnlySpan<byte> propertyName)
        {
            if (IsDefault) writer.WriteNull(propertyName);
            writer.WriteBase64String(propertyName, Span);
        }

        public static ClientTypeId ReadJson(ref Utf8JsonReader reader)
        {
            if (!TryReadJson(ref reader, out var result))
                throw new Exception($"ClientTypeId must be {Base64HashLength}-character base64 string");
            return result;
        }

        public static bool TryReadJson(ref Utf8JsonReader reader, out ClientTypeId output)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                output = default;
                return true;
            }
            if (reader.TokenType != JsonTokenType.String)
            {
                output = default;
                return false;
            }
            if (!reader.HasValueSequence && !reader.ValueIsEscaped)
            {
                return ClientTypeId.TryParse(reader.ValueSpan, out output);
            }
            else
            {
                Span<byte> buffer = stackalloc byte[Base64HashLength];
                var readBytes = reader.CopyString(buffer);
                if (readBytes != Base64HashLength)
                {
                    output = default;
                    return false;
                }
                return ClientTypeId.TryParse(buffer, out output);
            }
        }

        public override string ToString()
        {
            return Convert.ToBase64String(Span
#if !DotNetCore
                .ToArray()
#endif
            );
        }

        public override int GetHashCode() => (int)a;
        public override bool Equals(object? obj) => obj is ClientTypeId id && id.a == a && id.b == b;
        public bool Equals(ClientTypeId other) => other.a == a && other.b == b;
        public int CompareTo(ClientTypeId other) => a == other.a ? b.CompareTo(other.b) : a.CompareTo(other.a);
        public static bool operator ==(ClientTypeId left, ClientTypeId right) => left.Equals(right);
        public static bool operator !=(ClientTypeId left, ClientTypeId right) => !left.Equals(right);


        public sealed class JsonConverter : JsonConverter<ClientTypeId>
        {
            public override ClientTypeId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                ClientTypeId.ReadJson(ref reader);

            public override void Write(Utf8JsonWriter writer, ClientTypeId value, JsonSerializerOptions options) =>
                value.WriteJson(writer);
        }
    }
}
