using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel
{
    public class DefaultPropertySerialization : IPropertySerialization
    {
        static readonly Type? JsonPropertyNJ = Type.GetType("Newtonsoft.Json.JsonPropertyAttribute, Newtonsoft.Json");
        static readonly PropertyInfo? JsonPropertyNJPropertyName = JsonPropertyNJ?.GetProperty("PropertyName");
        public string ResolveName(PropertyInfo propertyInfo)
        {
            var bindAttribute = propertyInfo.GetCustomAttribute<BindAttribute>();
            if (bindAttribute != null)
            {
                if (!string.IsNullOrEmpty(bindAttribute.Name))
                {
                    return bindAttribute.Name!;
                }
            }

            // use JsonPropertyName name if Bind attribute is not present or doesn't specify it
            var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (!string.IsNullOrEmpty(jsonPropertyAttribute?.Name))
            {
                return jsonPropertyAttribute!.Name!;
            }

            if (JsonPropertyNJ is not null)
            {
                var jsonPropertyNJAttribute = propertyInfo.GetCustomAttribute(JsonPropertyNJ);
                if (jsonPropertyNJAttribute is not null)
                {
                    var name = (string?)JsonPropertyNJPropertyName!.GetValue(jsonPropertyNJAttribute);
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }
            }

            return propertyInfo.Name;
        }
    }
}
