using System.Reflection;
using Newtonsoft.Json;

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
                var jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
                if (!string.IsNullOrEmpty(jsonPropertyAttribute?.PropertyName))
                {
                    return jsonPropertyAttribute!.PropertyName!;
                }
            }

            return propertyInfo.Name;
        }
    }
}
