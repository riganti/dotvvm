using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Controls;

namespace DotVVM.Core.Common
{
    public class DotvvmApiOptions
    {
        public List<IKnownTypeMatcher> KnownTypeMatchers { get; } = new List<IKnownTypeMatcher>();

        public DotvvmApiOptions()
        {
            this.AddKnownAssembly(typeof(DotvvmApiOptions).GetTypeInfo().Assembly);
        }
    }

    public static class ApiHelpers
    {
        public static void AddKnownType(this DotvvmApiOptions options, params Type[] types)
        {
            options.KnownTypeMatchers.Add(new StaticKnownTypeMatcher(types));
        }

        public static void AddKnownAssembly(this DotvvmApiOptions options, params Assembly[] assemblies)
        {
            options.KnownTypeMatchers.Add(new AssemblyKnownTypeMatcher(assemblies));
        }

        public static void AddKnownNamespace(this DotvvmApiOptions options, string @namespace)
        {
            options.KnownTypeMatchers.Add(new NamespaceKnownTypeMatcher(@namespace));
        }

        public static bool IsKnownType(this DotvvmApiOptions options, Type type)
        {
            return options.KnownTypeMatchers.Any(matcher => matcher.IsKnownType(type));
        }
    }
}
