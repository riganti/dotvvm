using System;
using System.Collections.Generic;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public static class TextBoxHelper
    {
        private static readonly HashSet<Type> stringTypes = new HashSet<Type>() { typeof(string), typeof(Guid) };
        private static readonly HashSet<Type> numericTypes = new HashSet<Type>() { typeof(float), typeof(double), typeof(decimal), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
        private static readonly HashSet<Type> dateTypes = new HashSet<Type>() { typeof(DateTime) };

        public static bool CanHandleProperty(Type type)
        {
            return stringTypes.Contains(type) || numericTypes.Contains(type) || dateTypes.Contains(type);
        }
    }
}
