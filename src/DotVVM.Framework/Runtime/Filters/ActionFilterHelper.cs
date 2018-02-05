﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Filters
{
    public static class ActionFilterHelper
    {
        private static ConcurrentDictionary<(Type type, MemberInfo member, bool includeParents), object> cache_GetActionFilters = new ConcurrentDictionary<(Type type, MemberInfo member, bool includeParents), object>();
        // TODO make this ReadOnlySpan
        public static T[] GetActionFilters<T>(MemberInfo memberInfo, bool includeParents = true)
        {
            return (T[])cache_GetActionFilters.GetOrAdd((typeof(T), memberInfo, includeParents), data => {
                var result = new List<T>();
                do
                {
                    result.AddRange(data.member.CastTo<ICustomAttributeProvider>().GetCustomAttributes<T>());
                } while (data.includeParents && (data.member = data.member.DeclaringType?.GetTypeInfo()) != null);
                return result.ToArray();
            });
        }
    }
}
