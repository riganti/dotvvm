using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class EncryptedValuesWriter
    {
        JsonWriter writer;
        JsonSerializer serializer;
        Stack<int> propertyIndices = new Stack<int>();
        int virtualNests = 0;
        int propertyIndex = 0;

        public EncryptedValuesWriter(JsonWriter jsonWriter)
        {
            this.writer = jsonWriter;
            serializer = new JsonSerializer();
        }
        /// <summary>
        /// Indicates that serializer should nest to a inner object.
        /// Adds a new property to current object, and pushes the state to the stack.
        /// </summary>
        public void Nest()
        {
            propertyIndices.Push(propertyIndex + 1);
            propertyIndex = 0;
            virtualNests++;
        }

        /// <summary>
        /// Indicates that object has ended.
        /// Pops state from the stack.
        /// </summary>
        public void End()
        {
            if (virtualNests > 0) virtualNests--;
            else writer.WriteEndObject();
            propertyIndex = propertyIndices.Pop();
        }

        /// <summary>
        /// Ensure that the subtree is empty (did not contain any protected value) and clear it.
        /// </summary>
        public void ClearEmptyNest()
        {
            if (virtualNests <= 0) throw new Exception("");
            virtualNests--;
            propertyIndex = propertyIndices.Pop() - 1;
        }

        private void WritePropertyName(int index)
        {
            writer.WritePropertyName(index.ToString());
        }

        private void EnsureObjectStarted()
        {
            if (virtualNests > 0)
            {
                foreach (var p in propertyIndices.Take(virtualNests).Reverse())
                {
                    WritePropertyName(p - 1); // the property was not written, -1 to write it
                    writer.WriteStartObject();
                }
                virtualNests = 0;
            }
        }

        public bool IsVirtualNest() => virtualNests > 0;

        /// <summary>
        /// Write a value to the object.
        /// </summary>
        public void Value(object value)
        {
            EnsureObjectStarted();
            WritePropertyName(propertyIndex++);
            serializer.Serialize(writer, value);
        }
    }
}
