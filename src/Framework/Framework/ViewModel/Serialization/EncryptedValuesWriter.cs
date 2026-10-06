using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class EncryptedValuesWriter
    {
        Utf8JsonWriter writer;
        Stack<int> propertyIndices = new Stack<int>();
        Stack<string?> objectTypes = new();
        int virtualNests = 0;
        int lastPropertyIndex = -1;
        int suppress = 0;
        public int SuppressedLevel => suppress;

        public EncryptedValuesWriter(Utf8JsonWriter jsonWriter)
        {
            this.writer = jsonWriter;
        }

        public void Nest() => Nest(lastPropertyIndex + 1);

        /// <summary>
        /// Indicates that serializer should nest to a inner object.
        /// Adds a new property to current object, and pushes the state to the stack.
        /// </summary>
        public void Nest(int property)
        {
            if (suppress > 0) return;

            propertyIndices.Push(property);
            objectTypes.Push(null);
            lastPropertyIndex = -1;
            virtualNests++;
        }

        public void Suppress()
        {
            suppress++;
        }

        public void EndSuppress()
        {
            suppress--;
        }

        /// <summary>
        /// Indicates that object has ended.
        /// Pops state from the stack.
        /// </summary>
        public void End()
        {
            if (suppress > 0) return;

            if (virtualNests > 0)
            {
                virtualNests--;
            }
            else
            {
                writer.WriteEndObject();
            }
            lastPropertyIndex = propertyIndices.Pop();
            objectTypes.Pop();
        }

        /// <summary>
        /// Ensure that the subtree is empty (did not contain any protected value) and clear it.
        /// </summary>
        public void ClearEmptyNest()
        {
            if (suppress > 0) return;

            if (virtualNests <= 0) throw new NotSupportedException("There is no empty (virtual) nest to be cleared.");
            virtualNests--;
            lastPropertyIndex = propertyIndices.Pop();
            objectTypes.Pop();
        }

        private void WritePropertyName(int index)
        {
            writer.WritePropertyName(index.ToString());
        }

        private void EnsureObjectStarted()
        {
            if (virtualNests > 0)
            {
                bool first = true;
                foreach (var (p, type) in propertyIndices.Take(virtualNests).Reverse()
                    .Zip(objectTypes.Take(virtualNests).Reverse(), (p, type) => (p, type)))
                {
                    if (first && virtualNests == propertyIndices.Count)
                    {
                        // no wrapper object
                    }
                    else
                    {
                        WritePropertyName(p); // the property was not written, -1 to write it
                    }
                    first = false;

                    writer.WriteStartObject();
                    if (type is not null)
                        writer.WriteString("$type", type);
                }
                virtualNests = 0;
            }
        }

        public bool IsVirtualNest() => virtualNests > 0;

        /// <summary> Authenticate the object's layout only if a protected descendant materializes this subtree. </summary>
        public void SetType(string typeId)
        {
            if (suppress > 0) return;
            objectTypes.Pop();
            objectTypes.Push(typeId);
        }

        /// <summary>
        /// Write a value to the object.
        /// </summary>
        public void WriteValue(int propertyIndex, object? value, Type type, JsonSerializerOptions options)
        {
            if (suppress > 0) return;

            EnsureObjectStarted();
            WritePropertyName(propertyIndex);
            lastPropertyIndex = propertyIndex;
            if (!JsonPolymorphismInfo.ContainsPolymorphism(type))
            {
                JsonSerializer.Serialize(writer, value, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
                return;
            }
            Suppress();
            try
            {
                if (value is not null && (!ViewModelJsonConverter.CanConvertType(type) || !JsonPolymorphismInfo.IsPolymorphic(type)))
                    type = value.GetType();
                JsonSerializer.Serialize(writer, value, type, options);
            }
            finally
            {
                EndSuppress();
            }
        }

        public void WriteValue(int propertyIndex, object value)
        {
            if (suppress > 0) return;
            EnsureObjectStarted();
            WritePropertyName(propertyIndex);
            lastPropertyIndex = propertyIndex;
            JsonSerializer.Serialize(writer, value, DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe);
        }
    }
}
