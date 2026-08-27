using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
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
            if (virtualNests > 0 || json is null)
            {
                node = null;
                return false;
            }

            var name = index.ToString();
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
                    ThrowSecurityError();
            }
            lastPropertyIndex = stack.Pop().prop;
        }

        public void Suppress()
        {
            if (Suppressed) ThrowSecurityError();
            Suppressed = true;
        }

        public void EndSuppress()
        {
            if (!Suppressed) ThrowSecurityError();
            Suppressed = false;
        }

        public JsonNode? ReadValue(int property)
        {
            if (Suppressed) ThrowSecurityError();

            if (!Property(property, out var prop)) ThrowSecurityError();
            json!.Remove(property.ToString());
            return prop;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        static void ThrowSecurityError() => throw new SecurityException("Failed to deserialize viewModel encrypted values");
    }
}
