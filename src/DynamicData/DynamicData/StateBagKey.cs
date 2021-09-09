using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData
{
    public struct StateBagKey
    {

        public object Provider { get; private set; }

        public PropertyInfo Property { get; private set; }

        public StateBagKey(object provider, PropertyInfo property) : this()
        {
            Provider = provider;
            Property = property;
        }

    }
}