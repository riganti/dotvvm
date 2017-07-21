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
        int propertyIndex = -1;

        public EncryptedValuesWriter(JsonWriter jsonWriter)
        {
            this.writer = jsonWriter;
            serializer = new JsonSerializer();
        }

        public void Nest()
        {
            propertyIndices.Push(propertyIndex);
            propertyIndex = -1;
            virtualNests++;
        }

        public void End()
        {
            if (virtualNests > 0) virtualNests--;
            else writer.WriteEndObject();
            propertyIndex = propertyIndices.Pop();
        }

        public void ClearEmptyNest()
        {
            if (virtualNests <= 0) throw new NotSupportedException("There is no nest to be cleared.");
            virtualNests--;
            propertyIndex = propertyIndices.Pop();
        }

        public void IncrementIndex()
        {
            propertyIndex++;
        }

        private void WritePropertyName(int index)
        {
            writer.WritePropertyName(index.ToString());
        }

        private void EnsureObjectStarted()
        {
            if(virtualNests > 0)
            {
                foreach (var p in propertyIndices.Take(virtualNests).Reverse())
                {
                    WritePropertyName(p); // the property was not writter, -1 to write it
                    writer.WriteStartObject();
                }
                virtualNests = 0;
            }
        }

        public bool IsVirtualNest() => virtualNests > 0;

        public void Value(object value)
        {
            EnsureObjectStarted();
            WritePropertyName(propertyIndex);
            serializer.Serialize(writer, value);
        }
    }
}
