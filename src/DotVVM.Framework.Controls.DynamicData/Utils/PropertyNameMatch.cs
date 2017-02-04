using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData.Utils
{
    public class PropertyNameMatch : PropertyMatch
    {

        public string PropertyName { get; }

        public PropertyNameMatch(string propertyName)
        {
            PropertyName = propertyName;
        }


        public override bool IsMatch(PropertyInfo property)
        {
            return property.Name == PropertyName;
        }
    }
}