using System;
using System.Collections.Generic;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public static class TextBoxHelper
    {
        private static readonly HashSet<Type> stringTypes = new HashSet<Type>() { typeof(string), typeof(Guid) };
        private static readonly HashSet<Type> dateTypes = new HashSet<Type>() { typeof(DateTime), typeof(DateOnly), typeof(TimeOnly) };

        public static bool CanHandleProperty(Type type)
        {
            type = type.UnwrapNullableType();
            return stringTypes.Contains(type) || ReflectionUtils.IsNumericType(type) || dateTypes.Contains(type);
        }
    }
}
