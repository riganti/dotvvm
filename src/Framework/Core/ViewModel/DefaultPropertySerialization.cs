using System.Reflection;
using System.Text.Json.Serialization;

namespace DotVVM.Framework.ViewModel
{
    public class DefaultPropertySerialization : IPropertySerialization
    {
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

            if (string.IsNullOrEmpty(bindAttribute?.Name))
            {
                // use JsonProperty name if Bind attribute is not present or doesn't specify it
                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (!string.IsNullOrEmpty(jsonPropertyAttribute?.Name))
                {
                    return jsonPropertyAttribute!.Name!;
                }
            }

            return propertyInfo.Name;
        }
    }
}
