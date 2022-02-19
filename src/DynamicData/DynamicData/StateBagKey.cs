using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData
{
    public struct StateBagKey : IEquatable<StateBagKey>
    {
        public object Provider { get; private set; }

        public PropertyInfo Property { get; private set; }

        public StateBagKey(object provider, PropertyInfo property) : this()
        {
            Provider = provider;
            Property = property;
        }

        public override bool Equals(object? obj)
        {
            return obj is StateBagKey key && Equals(key);
        }

        public bool Equals(StateBagKey other)
        {
            return EqualityComparer<object>.Default.Equals(Provider, other.Provider)
                && EqualityComparer<PropertyInfo>.Default.Equals(Property, other.Property);
        }

        public override int GetHashCode()
        {
            return (Provider, Property).GetHashCode();
        }

        public static bool operator ==(StateBagKey left, StateBagKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StateBagKey left, StateBagKey right)
        {
            return !(left == right);
        }
    }
}
