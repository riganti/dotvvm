using System;
using System.Linq;
using System.Text;
using DotVVM.Core.Common;
using NJsonSchema;
using NJsonSchema.CodeGeneration;

namespace DotVVM.Tool.OpenApi
{
    public class CustomPropertyNameGenerator : IPropertyNameGenerator
    {
        private readonly Func<string, string> editCasing;

        public CustomPropertyNameGenerator(Func<string, string> editCasing)
        {
            this.editCasing = editCasing;
        }

        public string Generate(JsonSchemaProperty property)
        {
            if (property.ExtensionData != null
                && property.ExtensionData.TryGetValue(ApiConstants.DotvvmNameKey, out var name))
            {
                return name.ToString();
            }

            if (!property.Name.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' || c == '+'))
            {
                // crazy property name (like `+1` and `-1` in github), encode it in hex
                return editCasing("prop_" + BitConverter.ToString(Encoding.UTF8.GetBytes(property.Name)));
            }
            else
            {
                return editCasing(
                  property.Name.Replace("@", "")
                               .Replace(".", "-")
                               .Replace("+", "Plus")
                      ).Replace('-', '_');
            }
        }
    }
}
