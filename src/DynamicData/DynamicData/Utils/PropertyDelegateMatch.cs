using System;
using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData.Utils
{
    public class PropertyDelegateMatch : PropertyMatch
    {

        public Func<PropertyInfo, bool> Delegate { get; set; }

        public override bool IsMatch(PropertyInfo property)
        {
            return Delegate(property);
        }
    }
}