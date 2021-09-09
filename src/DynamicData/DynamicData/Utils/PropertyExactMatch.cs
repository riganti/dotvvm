using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData.Utils
{
    public class PropertyExactMatch : PropertyMatch
    {
        public PropertyInfo PropertyInfo { get; set; }

        public PropertyExactMatch(PropertyInfo property)
        {
            PropertyInfo = property;
        }


        public override bool IsMatch(PropertyInfo property)
        {
            return property.Equals(PropertyInfo);
        }
    }
}