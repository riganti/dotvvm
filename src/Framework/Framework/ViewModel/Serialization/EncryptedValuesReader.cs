using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class EncryptedValuesReader
    {
        Stack<(int prop, JsonObject? obj)> stack = new();
        int virtualNests = 0;
        int lastPropertyIndex = -1;
        public bool Suppressed { get; private set; } = false;

        public EncryptedValuesReader(JsonObject json)
        {
            stack.Push((0, json));
        }

        private JsonObject? json => stack.Peek().obj;

        private bool Property(int index, out JsonNode? node)
        {
            var name = index.ToString();
            if (virtualNests > 0 || json is null)
            {
                node = null;
                return false;
            }

            return json.TryGetPropertyValue(name, out node);
        }

        public void Nest() => Nest(lastPropertyIndex + 1);

        public void Nest(int property)
        {
            if (Suppressed)
                return;

            if (Property(property, out var prop))
            {
                Debug.Assert(prop is JsonObject, $"Unexpected prop {property}: {prop}");
                json?.Remove(property.ToString());
            }
            else
            {
                virtualNests++;
            }
            // remove read nodes and then make sure that JObject is empty
            stack.Push((property, (JsonObject?)prop));
            lastPropertyIndex = -1;
        }

        public void AssertEnd()
        {
            if (Suppressed)
                return;

            if (virtualNests > 0)
            {
                virtualNests--;
            }
            else
            {
                if (json?.Count > 0)
                    throw SecurityError();
            }
            lastPropertyIndex = stack.Pop().prop;
        }

        public void Suppress()
        {
            if (Suppressed) throw SecurityError();
            Suppressed = true;
        }

        public void EndSuppress()
        {
            if (!Suppressed) throw SecurityError();
            Suppressed = false;
        }

        public JsonNode? ReadValue(int property)
        {
            if (Suppressed) throw SecurityError();

            if (!Property(property, out var prop)) throw SecurityError();
            json!.Remove(property.ToString());
            return prop;
        }

        Exception SecurityError() => new SecurityException("Failed to deserialize viewModel encrypted values");
    }
}
