using System;
using System.Globalization;
using System.Reflection;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Configuration
{
    public sealed class CustomPrimitiveTypeRegistration
    {
        public Type Type { get; }

        public Func<string, ParseResult> TryParseMethod { get; }

        public Func<object, string> ToStringMethod { get; }

        internal CustomPrimitiveTypeRegistration(Type type)
        {
            if (ReflectionUtils.IsCollection(type) || ReflectionUtils.IsDictionary(type))
            {
                throw new DotvvmConfigurationException($"The type {type} is marked with {nameof(CustomPrimitiveTypeAttribute)}, but it cannot be used as a custom primitive type. Custom primitive types cannot be collections, dictionaries, and cannot be primitive types already supported by DotVVM.");
            }

            Type = type;

            TryParseMethod = ResolveTryParseMethod(type);
            ToStringMethod = ResolveToStringMethod(type);
        }

        internal static Func<string, ParseResult> ResolveTryParseMethod(Type type)
        {
            var tryParseMethod = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null,
                                     new[] { typeof(string), typeof(IFormatProvider), type.MakeByRefType() }, null)
                                 ?? type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null,
                                     new[] { typeof(string), type.MakeByRefType() }, null)
                                 ?? throw new DotvvmConfigurationException($"The type {type} is marked with {nameof(CustomPrimitiveTypeAttribute)} but it does not contain a public static method TryParse(string, IFormatProvider, out {type}) or TryParse(string, out {type})!");

            var hasFormatProvider = tryParseMethod.GetParameters().Length == 3;
            return arg => {
                var args = hasFormatProvider
                    ? new object[] { arg, CultureInfo.InvariantCulture, null }
                    : new object[] { arg, null };
                return (bool)tryParseMethod.Invoke(null, args) ? new ParseResult(args[args.Length - 1]) : ParseResult.Failed;
            };
        }

        internal static Func<object, string> ResolveToStringMethod(Type type)
        {
            var toStringMethod = type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null,
                                     new[] { typeof(string), typeof(IFormatProvider) }, null)
                                 ?? type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null,
                                     new[] { typeof(IFormatProvider) }, null)
                                 ?? type.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null,
                                    new Type[] { }, null)
                                 ?? throw new DotvvmConfigurationException($"The type {type} is marked with {nameof(CustomPrimitiveTypeAttribute)} but it does not contain a public method ToString(string, IFormatProvider), ToString(IFormatProvider), or ToString()!");
            var parameterCount = toStringMethod.GetParameters().Length;
            return arg => {
                var args = parameterCount switch {
                    2 => new object?[] { null, CultureInfo.InvariantCulture },
                    1 => new object?[] { CultureInfo.InvariantCulture },
                    _ => new object?[] { }
                };
                return (string)toStringMethod.Invoke(arg, args);
            };
        }

        public record ParseResult(object? Result = null)
        {
            public bool Successful { get; init; } = true;

            public static readonly ParseResult Failed = new ParseResult() { Successful = false };
        }
    }
}
