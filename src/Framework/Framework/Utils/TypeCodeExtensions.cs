using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Utils
{
    public static class TypeCodeExtensions
    {
        private static readonly IReadOnlyDictionary<Type, string> TypeKeywords = new Dictionary<Type, string>
        {
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort",
            [typeof(int)] = "int",
            [typeof(uint)] = "uint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(char)] = "char",
            [typeof(string)] = "string",
            [typeof(object)] = "object",
            [typeof(void)] = "void"
        };

        public static string ToCode(this Type? type, bool stripNamespace = false)
        {
            if (type is null)
            {
                return "null";
            }
            return ToCodeCore(type, stripNamespace);
        }

        private static string ToCodeCore(Type type, bool stripNamespace)
        {
            if (type.IsByRef)
            {
                return $"{ToCodeCore(type.GetElementType()!, stripNamespace)}&";
            }
            if (type.IsPointer)
            {
                return $"{ToCodeCore(type.GetElementType()!, stripNamespace)}*";
            }
            if (type.IsArray)
            {
                var rank = new string(',', type.GetArrayRank() - 1);
                return $"{ToCodeCore(type.GetElementType()!, stripNamespace)}[{rank}]";
            }
            if (Nullable.GetUnderlyingType(type) is { } nullableType)
            {
                return $"{ToCodeCore(nullableType, stripNamespace)}?";
            }
            if (TypeKeywords.TryGetValue(type, out var keyword))
            {
                return keyword;
            }
            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            var typeName = type.Name;
            var genericTick = typeName.IndexOf('`');
            if (genericTick >= 0)
            {
                typeName = typeName[..genericTick];
            }

            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                if (type.IsNested && type.DeclaringType?.IsGenericType == true)
                {
                    genericArguments = genericArguments.Skip(type.DeclaringType.GetGenericArguments().Length).ToArray();
                }

                if (genericArguments.Length > 0)
                {
                    typeName += $"<{string.Join(", ", genericArguments.Select(a => ToCodeCore(a, stripNamespace)))}>";
                }
            }

            var prefix = type.IsNested
                ? $"{ToCodeCore(type.DeclaringType!, stripNamespace)}."
                : (!stripNamespace && !string.IsNullOrEmpty(type.Namespace) ? $"{type.Namespace}." : "");

            return prefix + typeName;
        }
    }
}
