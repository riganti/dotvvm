using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Controls
{
    internal class DotvvmControlDebugJsonConverter() : GenericWriterJsonConverter<IDotvvmObjectLike>((w, objInterface, options) => {
        var obj = objInterface.Self;
        w.WriteStartObject();

        w.WritePropertyName("Control");
        w.WriteStringValue(obj.GetType().Name);
        
        w.WriteStartObject("Properties");
        foreach (var kvp in obj.Properties.OrderBy(p => (p.Key.DeclaringType.IsAssignableFrom(obj.GetType()), p.Key.Name)))
        {
            var p = kvp.Key;
            var rawValue = kvp.Value;
            var isAttached = !p.DeclaringType.IsAssignableFrom(obj.GetType());
            var name = isAttached ? p.DeclaringType.Name + "." + p.Name : p.Name;
            if (rawValue is null)
                w.WriteNull(name);
            else if (rawValue is IBinding)
                w.WriteString(name, rawValue.ToString());
            else
            {
                w.WritePropertyName(name);
                try
                {
                    JsonSerializer.Serialize(w, rawValue, options);
                }
                catch (Exception ex)
                {
                    // don't crash error page when there is some error in serialization
                    w.WriteStringValue($"Error serializing property {name}: {ex.Message}");
                }
            }
        }
        w.WriteEndObject();

        if (obj is DotvvmControl control)
        {
            w.WritePropertyName("LifecycleRequirements");
            w.WriteStringValue(control.LifecycleRequirements.ToString());
        }

        w.WriteEndObject();
    })
    {
    }
}
