using System;
using System.Collections.Generic;
using System.Reflection;
using DotVVM.Framework.Controls;

namespace DotVVM.Core.Common
{
    public class DotvvmApiOptions
    {
        public List<Type> KnownTypes { get; }
            = new List<Type> { typeof(IGridViewDataSet<>), typeof(IPagingOptions), typeof(ISortingOptions),
                typeof(IRowEditOptions), typeof(GridViewDataSet<>), typeof(PagingOptions), typeof(SortingOptions), typeof(RowEditOptions)};
    }

    public static class ApiHelpers
    {
        public static bool IsKnownType(this DotvvmApiOptions options, Type type)
        {
            var result = options.KnownTypes.Contains(type);

            var typeInfo = type.GetTypeInfo();
            if (!result && typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                return options.IsKnownType(type.GetGenericTypeDefinition());
            }

            return result;
        }
    }
}
