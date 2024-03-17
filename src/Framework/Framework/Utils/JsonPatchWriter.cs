// using System.Linq;
// using System;
// using System.Text.Json;
// using System.Diagnostics;
// using System.Buffers;
// using System.Collections.Generic;

// namespace DotVVM.Framework.Utils
// {
//     ref struct JsonPatchWriter
//     {
//         static void CopyValue(ref Utf8JsonReader reader, Utf8JsonWriter writer)
//         {
//             Debug.Assert(reader.TokenType != JsonTokenType.PropertyName);

//             if (reader.TokenType is not JsonTokenType.StartArray and not JsonTokenType.StartObject)
//             {
//                 if (reader.HasValueSequence)
//                     writer.WriteRawValue(reader.ValueSequence);
//                 else
//                     writer.WriteRawValue(reader.ValueSpan);

//                 return;
//             }

//             var depth = reader.CurrentDepth;
//             while (reader.CurrentDepth >= depth)
//             {
//                 switch (reader.TokenType)
//                 {
//                     case JsonTokenType.False:
//                     case JsonTokenType.True:
//                     case JsonTokenType.Null:
//                     case JsonTokenType.String:
//                     case JsonTokenType.Number: {
//                         if (reader.HasValueSequence)
//                             writer.WriteRawValue(reader.ValueSequence);
//                         else
//                             writer.WriteRawValue(reader.ValueSpan);
//                         break;
//                     }
//                     case JsonTokenType.PropertyName: {
//                         var length = reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length;
//                         Span<byte> buffer = length <= 1024 ? stackalloc byte[(int)length] : new byte[length];
//                         var realLength = reader.CopyString(buffer);
//                         writer.WritePropertyName(buffer.Slice(0, realLength));
//                         break;
//                     }
//                     case JsonTokenType.StartArray: {
//                         writer.WriteStartArray();
//                         break;
//                     }
//                     case JsonTokenType.EndArray: {
//                         writer.WriteEndArray();
//                         break;
//                     }
//                     case JsonTokenType.StartObject: {
//                         writer.WriteStartObject();
//                         break;
//                     }
//                     case JsonTokenType.EndObject: {
//                         writer.WriteEndObject();
//                         break;
//                     }
//                     default: {
//                         throw new JsonException($"Unexpected token {reader.TokenType}.");
//                     }
//                 }
//                 reader.Read();
//             }
//         }

//         private readonly Utf8JsonWriter writer;
//         private List<JsonElement> patchStack;
//         private Span<byte> nameBuffer;
//         private byte[] nameBufferRented;

//         private JsonPatchWriter(
//             Utf8JsonWriter writer,
//             JsonElement patch,
//             Span<byte> nameBuffer
//         )
//         {
//             this.writer = writer;
//         }

//         Span<byte> ReadName(ref Utf8JsonReader reader)
//         {
//             var length = reader.CopyString(nameBuffer);
//             if (length < nameBuffer.Length)
//             {
//                 return nameBuffer.Slice(length);
//             }
//             var newBuffer = ArrayPool<byte>.Shared.Rent(length);
//             nameBuffer.CopyTo(newBuffer);
//             if (nameBufferRented is {})
//                 ArrayPool<byte>.Shared.Return(nameBufferRented);
//             nameBuffer = newBuffer;
//             nameBufferRented = newBuffer;
//             return ReadName(ref reader);
//         }

//         private void Patch(ref Utf8JsonReader original, JsonElement patchValue)
//         {
//             var patchKind = patchValue.ValueKind;
//             if (patchKind == JsonValueKind.Object && original.TokenType == JsonTokenType.StartObject)
//             {
//                 PatchObject(ref original, patchValue);
//             }
//             else if (patchKind == JsonValueKind.Array && original.TokenType == JsonTokenType.StartArray)
//             {
//                 PatchArray(ref original, patchValue);
//             }
//             else
//             {
//                 patchValue.WriteTo(writer);
//             }
//         }

//         void PatchObject(ref Utf8JsonReader original, JsonElement patch)
//         {
//             original.AssertToken(JsonTokenType.StartObject);
//             if (patch.ValueKind != JsonValueKind.Object)
//             {
//                 patch.WriteTo(writer);
//                 return;
//             }
//             writer.WriteStartObject();
//             original.Read();

//             var patchedProperties = 0;
//             while (original.TokenType == JsonTokenType.PropertyName)
//             {
//                 var propertyName = ReadName(ref original);
//                 original.Read();
//                 writer.WritePropertyName(propertyName);

//                 if (!patch.TryGetProperty(propertyName, out var patchValue))
//                 {
//                     CopyValue(ref original, writer);
//                     continue;
//                 }

//                 patchedProperties += 1;

//                 Patch(ref original, patchValue);
//             }
//             original.AssertToken(JsonTokenType.EndObject);

//             var remainingProperties = -patchedProperties;
//             foreach (var p in patch.EnumerateObject())
//             {
//                 remainingProperties += 1;
//             }
//             if (remainingProperties > 0)
//             {
//                 throw new JsonException("Patching failed");
//             }

//             writer.WriteEndObject();
//         }

//         void PatchArray(ref Utf8JsonReader original, JsonElement patch)
//         {
//             using var patchEnumerator = patch.EnumerateArray();
//             original.AssertRead(JsonTokenType.StartArray);
//             writer.WriteStartArray();

//             while (original.TokenType != JsonTokenType.EndArray)
//             {
//                 if (!patchEnumerator.MoveNext())
//                 {
//                     while (original.TokenType != JsonTokenType.EndArray)
//                     {
//                         original.Skip();
//                         original.Read();
//                     }
//                 }

//                 var patchKind = patchEnumerator.Current.ValueKind;
//                 var tokenType = original.TokenType;
//                 if (patchKind == JsonValueKind.Object && tokenType == JsonTokenType.StartObject)
//                 {
//                     PatchObject(ref original, patchEnumerator.Current);
//                 }
//                 else if (patchKind == JsonValueKind.Array && tokenType == JsonTokenType.StartArray)
//                 {
//                     PatchArray(ref original, patchEnumerator.Current);
//                 }
//                 else
//                 {
//                     patchEnumerator.Current.WriteTo(writer);
//                 }
//                 original.Read();
//             }

//             while (patchEnumerator.MoveNext())
//             {
//                 patchEnumerator.Current.WriteTo(writer);
//             }

//             writer.WriteEndArray();
//         }
//     }
// }
