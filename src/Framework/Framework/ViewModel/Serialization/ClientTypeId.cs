// using System;
// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;
// using System.Text.Json;
// using DotVVM.Framework.Utils;

// namespace DotVVM.Framework.ViewModel.Serialization
// {
//     [StructLayout(LayoutKind.Explicit)]
//     readonly struct ClientTypeId: IEquatable<ClientTypeId>, IComparable<ClientTypeId>
//     {
//         // first byte 
//         [FieldOffset(0)]
//         readonly ulong a;
//         [FieldOffset(8)]
//         readonly ulong b;

//         [FieldOffset(0)]
//         readonly byte controlByte;
//         [FieldOffset(1)]
//         readonly byte dataByte1;
//         private ClientTypeId(ulong a, ulong b)
//         {
//             this.a = a;
//             this.b = b;
//         }

//         private ClientTypeId(bool isHash, ReadOnlySpan<byte> data)
//         {
//             if (data.Length > 15) throw new ArgumentException("Data too long");
//             controlByte = (byte)(data.Length | (isHash ? 0x10 : 0));
// #if DotNetCore
//             data.CopyTo(MemoryMarshal.CreateSpan(ref dataByte1, 15));
// #else
//             unsafe
//             {
//                 fixed (byte* ptr = &dataByte1)
//                 {
//                     data.CopyTo(new Span<byte>(ptr, 15));
//                 }
//             }
// #endif
//         }

//         struct Utf8StringCtor {}
//         private ClientTypeId(ReadOnlySpan<byte> utf8Hash, Utf8StringCtor _)
//         {
//             if (utf8Hash.Length != 16) throw new ArgumentException("Hash must be 16 bytes long");
//             controlByte = (byte)(12 | 0x10);
// #if DotNetCore
//             System.Buffers.Text.Base64.DecodeFromUtf8(utf8Hash, MemoryMarshal.CreateSpan(ref dataByte1, 12), out var _, out var _);
// #else
//             unsafe
//             {
//                 fixed (byte* ptr = &dataByte1)
//                 {
//                     System.Buffers.Text.Base64.DecodeFromUtf8(utf8Hash, new Span<byte>(ptr, 12), out var _, out var _);
//                 }
//             }
// #endif
//         }

//         public static ClientTypeId CreateHash(ReadOnlySpan<byte> data) => new ClientTypeId(true, data);
//         public static ClientTypeId CreateString(ReadOnlySpan<byte> data) => new ClientTypeId(false, data);
//         public static ClientTypeId Parse(ReadOnlySpan<byte> utf8) =>
//             utf8.Length == 16 ? new ClientTypeId(utf8, default(Utf8StringCtor)) : new ClientTypeId(false, utf8);

//         byte Length => (byte)(controlByte & 0xf);
//         bool IsHash => ((controlByte >> 4) & 1) != 0;
//         bool IsEmpty => controlByte == 0;

//         ReadOnlySpan<byte> Data =>
// #if DotNetCore
//             MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in dataByte1), Length);
// #else
//             throw new NotImplementedException();
// #endif

//         public void WriteJson(Utf8JsonWriter writer)
//         {
//             if (IsEmpty) writer.WriteNullValue();
//             else if (IsHash)
//                 writer.WriteBase64StringValue(Data);
//             else
//                 writer.WriteStringValue(Data);
//         }
//         public void WriteJson(Utf8JsonWriter writer, ReadOnlySpan<byte> propertyName)
//         {
//             if (IsEmpty) writer.WriteNull(propertyName);
//             else if (IsHash)
//                 writer.WriteBase64String(propertyName, Data);
//             else
//                 writer.WriteString(propertyName, Data);
//         }

//         public static ClientTypeId ReadJson(ref Utf8JsonReader reader)
//         {
//             if (reader.TokenType == JsonTokenType.Null) return default;
//             if (reader.TokenType != JsonTokenType.String) throw new JsonException("Expected string");
//             Span<byte> buffer = stackalloc byte[16];
//             var readBytes = reader.CopyString(buffer);
//             return readBytes == 16 ? new ClientTypeId(buffer, default(Utf8StringCtor)) : new ClientTypeId(false, buffer.Slice(0, readBytes));
//         }

//         public static bool TryReadJson(ref Utf8JsonReader reader, out ClientTypeId output)
//         {
//             if (reader.TokenType == JsonTokenType.Null)
//             {
//                 output = default;
//                 return true;
//             }
//             if (reader.TokenType != JsonTokenType.String)
//             {
//                 output = default;
//                 return false;
//             }
//             const int maxLength = 16 * 6; // JsonConstants.MaxExpansionFactorWhileEscaping
//             if (reader.GetValueLength() > maxLength)
//             {
//                 output = default;
//                 return false;
//             }
//             Span<byte> buffer = stackalloc byte[maxLength];
//             var readBytes = reader.CopyString(buffer);
//             if (readBytes > 16)
//             {
//                 output = default;
//                 return false;
//             }
//             output = readBytes == 16 ? new ClientTypeId(buffer, default(Utf8StringCtor)) : new ClientTypeId(false, buffer.Slice(0, readBytes));
//             return true;
//         }

//         public override string ToString()
//         {
//             if (IsEmpty) return "[Empty]";
//             if (IsHash)
//                 return Convert.ToBase64String(Data
// #if !DotNetCore
//                     .ToArray()
// #endif
//                 );
//             else
//                 return StringUtils.Utf8Decode(Data);
//         }
//         public override int GetHashCode() => (a, b).GetHashCode();
//         public override bool Equals(object? obj) => obj is ClientTypeId id && id.a == a && id.b == b;
//         public bool Equals(ClientTypeId other) => other.a == a && other.b == b;
//         public int CompareTo(ClientTypeId other) => a == other.a ? b.CompareTo(other.b) : a.CompareTo(other.a);
//         public static bool operator ==(ClientTypeId left, ClientTypeId right) => left.Equals(right);
//         public static bool operator !=(ClientTypeId left, ClientTypeId right) => !left.Equals(right);
//     }
// }
