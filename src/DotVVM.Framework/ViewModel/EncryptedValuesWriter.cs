using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel
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

        public void Nest()
        {
            propertyIndices.Push(propertyIndex + 1);
            propertyIndex = 0;
            virtualNests++;
        }

        public void End()
        {
            if (virtualNests > 0) virtualNests--;
            else writer.WriteEndObject();
            propertyIndex = propertyIndices.Pop();
        }

        private void WritePropertyName(int index)
        {
            writer.WritePropertyName(index.ToString());
        }

        private void EnsureObjectStarted()
        {
            if(virtualNests > 0)
            {
                foreach (var p in propertyIndices.Skip(propertyIndices.Count - virtualNests))
                {
                    WritePropertyName(p - 1); // the property was not writter, -1 to write it
                    writer.WriteStartObject();
                }
                virtualNests = 0;
            }
        }

        public bool IsVirtualNest() => virtualNests > 0;

        public void Value(object value)
        {
            EnsureObjectStarted();
            WritePropertyName(propertyIndex++);
            serializer.Serialize(writer, value);
        }
    }
}
