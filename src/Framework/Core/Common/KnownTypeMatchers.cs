using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DotVVM.Core.Common
{
    public interface IKnownTypeMatcher
    {
        bool IsKnownType(Type type);
    }

    public class NamespaceKnownTypeMatcher : IKnownTypeMatcher
    {
        private readonly string @namespace;

        public NamespaceKnownTypeMatcher(string @namespace)
        {
            this.@namespace = @namespace;
        }

        public bool IsKnownType(Type type)
        {
            return type.Namespace is not null && type.Namespace.StartsWith(@namespace, StringComparison.Ordinal);
        }
    }

    public class StaticKnownTypeMatcher : IKnownTypeMatcher
    {
        private readonly Type[] knownTypes;

        public StaticKnownTypeMatcher(IEnumerable<Type> knownTypes)
        {
            this.knownTypes = knownTypes.ToArray();
        }

        public bool IsKnownType(Type type)
        {
            var result = knownTypes.Contains(type);

            var typeInfo = type.GetTypeInfo();
            if (!result && typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                return IsKnownType(type.GetGenericTypeDefinition());
            }

            return result;
        }
    }

    public class AssemblyKnownTypeMatcher : IKnownTypeMatcher
    {
        private readonly Assembly[] assemblies;

        public AssemblyKnownTypeMatcher(IEnumerable<Assembly> assemblies)
        {
            this.assemblies = assemblies.ToArray();
        }

        public bool IsKnownType(Type type)
        {
            return assemblies.Contains(type.GetTypeInfo().Assembly);
        }
    }
}
