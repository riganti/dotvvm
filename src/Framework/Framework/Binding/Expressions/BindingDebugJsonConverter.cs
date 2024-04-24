using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Binding.Expressions
{
    internal class BindingDebugJsonConverter(bool detailed): GenericWriterJsonConverter<IBinding>((writer, obj, options) => {
        if (detailed)
        {
            writer.WriteStartObject();
            writer.WriteString("ToString"u8, obj.ToString());
            var props = (obj as ICloneableBinding)?.GetAllComputedProperties() ?? Enumerable.Empty<IBinding>();
            foreach (var p in props)
            {
                var name = p.GetType().Name;
                writer.WritePropertyName(name);
                JsonSerializer.Serialize(writer, p, options);
            }
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStringValue(obj?.ToString());
        }
    })
    {
        public BindingDebugJsonConverter() : this(false) { }
    }
}
