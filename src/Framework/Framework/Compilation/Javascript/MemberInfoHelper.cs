using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotVVM.Framework.Compilation.Javascript
{
    static class MethodFindingHelper
    {
        public static MethodBase GetMethodFromExpression<T>(Expression<Func<T>> expression)
        {
            return GetMethodFromExpression((Expression)expression);
        }
        static MethodBase GetMethodFromExpression(Expression expression)
        {
            var originalExpression = expression;
            if (expression.NodeType == ExpressionType.Lambda)
                expression = ((LambdaExpression)expression).Body;

            while (expression.NodeType == ExpressionType.Convert && expression is UnaryExpression unary && unary.Method is null)
                expression = unary.Operand;

            if (TryGetPropertyFromExpression(expression) is { } property)
                return property switch {
                    PropertyInfo {GetMethod:{}} p => p.GetMethod,
                    _ => throw new NotSupportedException($"Unsupported member type {property}")
                };

            return expression switch {
                MethodCallExpression call => call.Method,
                BinaryExpression { Method: { } } binary => binary.Method,
                BinaryExpression { NodeType: ExpressionType.Assign } assign =>
                    TryGetPropertyFromExpression(assign.Left) switch {
                        PropertyInfo {SetMethod: {}} p => p.SetMethod,
                        null => throw new NotSupportedException($"Cannot get member from {originalExpression}"),
                        var p => throw new NotSupportedException($"Unsupported assigned member type {p}")
                    },
                UnaryExpression { Method: {}} unary => unary.Method,
                NewExpression { Constructor: { } } newExpression => newExpression.Constructor,
                _ => throw new NotSupportedException($"Cannot get member from {originalExpression}")
            };
        }

        static MemberInfo? TryGetPropertyFromExpression(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
                return memberExpression.Member;
            else if (expression is IndexExpression index)
                return index.Indexer;
            else
                return null;
        }

        public static MethodInfo Genericize(MethodInfo m)
        {
            if (m.DeclaringType.IsGenericType)
            {
                var def = m.DeclaringType.GetGenericTypeDefinition();
                var args = m.DeclaringType.GetGenericArguments();
                var parameters = def.GetGenericArguments();
                var newType = def.MakeGenericType(
                    args.Zip(parameters, (argument, parameter) =>
                        argument == typeof(Generic) ? parameter : argument).ToArray()
                );
                var methods = newType.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                m = methods.Single(mm => m.MetadataToken == mm.MetadataToken);

            }
            if (m.IsGenericMethod)
            {
                var def = m.GetGenericMethodDefinition();
                var args = m.GetGenericArguments();
                var parameters = def.GetGenericArguments();
                var newType = def.MakeGenericMethod(
                    args.Zip(parameters, (argument, parameter) =>
                        argument == typeof(Generic) ? parameter : argument).ToArray()
                );
                return def;
            }
            return m;
        }


        public enum Generic {}
    }
}
