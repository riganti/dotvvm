using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Filters
{
    public static class ActionFilterHelper
    {
        public static List<T> GetActionFilters<T>(MemberInfo memberInfo, bool includeParents = true)
        {
            var result = new List<T>();
            do
            {
                result.AddRange(memberInfo.CastTo<ICustomAttributeProvider>().GetCustomAttributes<T>());
            } while (includeParents && (memberInfo = memberInfo.DeclaringType?.GetTypeInfo()) != null);
            return result;
        }
    }
}
