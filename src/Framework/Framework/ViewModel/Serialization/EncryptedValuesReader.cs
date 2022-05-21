using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public class EncryptedValuesReader
    {
        Stack<(int prop, JObject? obj)> stack = new();
        int virtualNests = 0;
        int lastPropertyIndex = -1;
        public bool Suppressed { get; private set; } = false;

        public EncryptedValuesReader(JObject json)
        {
            stack.Push((0, json));
        }

        private JObject? json => stack.Peek().obj;

        private JProperty? Property(int index)
        {
            var name = index.ToString();
            return virtualNests == 0 ? json?.Property(name) : null;
        }

        public void Nest() => Nest(lastPropertyIndex + 1);

        public void Nest(int property)
        {
            if (Suppressed)
                return;

            var prop = Property(property);
            if (prop != null)
            {
                Debug.Assert(prop.Value.Type == JTokenType.Object);
            }
            else
            {
                virtualNests++;
            }
            stack.Push((property, (JObject?)prop?.Value));
            // remove read nodes and then make sure that JObject is empty
            prop?.Remove();
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
                if (json!.Properties().Count() > 0)
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

        public JToken ReadValue(int property)
        {
            if (Suppressed) throw SecurityError();

            var prop = Property(property);
            if (prop == null) throw SecurityError();
            prop.Remove();
            return prop.Value;
        }

        Exception SecurityError() => new SecurityException("Failed to deserialize viewModel encrypted values");
    }
}
