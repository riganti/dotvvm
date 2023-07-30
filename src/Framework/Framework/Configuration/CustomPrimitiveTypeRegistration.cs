using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
            ToStringMethod = typeof(IFormattable).IsAssignableFrom(type)
                ? obj => ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture)
                : obj => obj.ToString()!;
        }

        internal static Func<string, ParseResult> ResolveTryParseMethod(Type type)
        {
            var tryParseMethod = type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null,
                                     new[] { typeof(string), typeof(IFormatProvider), type.MakeByRefType() }, null)
                                 ?? type.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null,
                                     new[] { typeof(string), type.MakeByRefType() }, null)
                                 ?? throw new DotvvmConfigurationException($"The type {type} is marked with {nameof(CustomPrimitiveTypeAttribute)} but it does not contain a public static method TryParse(string, IFormatProvider, out {type}) or TryParse(string, out {type})!");

            var inputParameter = Expression.Parameter(typeof(string), "arg");
            var resultVariable = Expression.Variable(type, "result");

            var arguments = new Expression?[]
                {
                    inputParameter,
                    tryParseMethod.GetParameters().Length == 3
                        ? Expression.Constant(CultureInfo.InvariantCulture)
                        : null,
                    resultVariable
                }
                .Where(a => a != null)
                .Cast<Expression>()
                .ToArray();
            var call = Expression.Call(tryParseMethod, arguments);

            var body = Expression.Block(
                new[] { resultVariable },
                Expression.Condition(
                    Expression.IsTrue(call),
                    Expression.New(typeof(ParseResult).GetConstructor(new[] { typeof(object) })!, Expression.Convert(resultVariable, typeof(object))),
                    Expression.Constant(ParseResult.Failed)
                )
            );
            return Expression.Lambda<Func<string, ParseResult>>(body, inputParameter).Compile();
        }

        public record ParseResult(object? Result = null)
        {
            public bool Successful { get; init; } = true;

            public static readonly ParseResult Failed = new ParseResult() { Successful = false };
        }
    }
}
