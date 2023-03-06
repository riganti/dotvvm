using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotVVM.Framework.Compilation.Javascript
{
    public static class MethodFindingHelper
    {
        public static MethodBase GetMethodFromExpression<T>(Expression<Func<T>> expression)
        {
            return Generalize(GetMethodFromExpression(expression.Body));
        }
        public static MethodBase GetMethodFromExpression(Expression<Action> expression)
        {
            return Generalize(GetMethodFromExpression(expression.Body));
        }
        public static PropertyInfo GetPropertyFromExpression<T>(Expression<Func<T>> expression)
        {
            var p = TryGetPropertyFromExpression(UnwrapConversions(expression.Body)) as PropertyInfo;
            if (p is null)
                throw new NotSupportedException($"Cannot get PropertyInfo from {expression.Body}");
            return Generalize(p);
        }
        static Expression UnwrapConversions(Expression e)
        {
            if (e.NodeType == ExpressionType.Lambda)
                e = ((LambdaExpression)e).Body;

            while (e.NodeType == ExpressionType.Convert && e is UnaryExpression unary && unary.Method is null)
                e = unary.Operand;
            return e;
        }
        static MethodBase GetMethodFromExpression(Expression expression)
        {
            var originalExpression = expression;
            expression = UnwrapConversions(expression);

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
            else if (expression is MethodCallExpression { Method: { IsSpecialName: true } method } && method.Name.StartsWith("get_"))
                return method.DeclaringType.GetProperty(method.Name.Substring(4));
            else if (expression is IndexExpression index)
                return index.Indexer;
            else
                return null;
        }

        public static Type Generalize(Type t)
        {
            if (!t.IsGenericType)
                return t;
            var args = t.GetGenericArguments();
            if (!args.Any(a => a.DeclaringType == typeof(Generic)))
                return t;
            var def = t.GetGenericTypeDefinition();
            var parameters = def.GetGenericArguments();
            var newType = def.MakeGenericType(
                args.Zip(parameters, (argument, parameter) =>
                    argument.DeclaringType == typeof(Generic) ? parameter : argument).ToArray()
            );
            return newType;
        }

        public static PropertyInfo Generalize(PropertyInfo p)
        {
            var newType = Generalize(p.DeclaringType);
            if (newType == p.DeclaringType)
                return p;

            var properties = newType.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return properties.Single(pp => p.MetadataToken == pp.MetadataToken);
        }

        public static MethodBase Generalize(MethodBase m) =>
            m switch {
                MethodInfo mi => Generalize(mi),
                ConstructorInfo ci => Generalize(ci),
                _ => throw new NotSupportedException($"Unsupported method type {m}")
            };
        public static ConstructorInfo Generalize(ConstructorInfo c)
        {
            var newType = Generalize(c.DeclaringType);
            if (newType == c.DeclaringType)
                return c;
            var constructors = newType.GetConstructors(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return constructors.Single(mm => c.MetadataToken == mm.MetadataToken);
        }
        public static MethodInfo Generalize(MethodInfo m)
        {
            if (m.DeclaringType.IsGenericType)
            {
                var newType = Generalize(m.DeclaringType);
                if (newType != m.DeclaringType)
                {
                    var methods = newType.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    m = methods.Single(mm => m.MetadataToken == mm.MetadataToken);
                }
            }
            if (m.IsGenericMethod)
            {
                var args = m.GetGenericArguments();
                if (args.Any(a => a.DeclaringType == typeof(Generic)))
                {
                    var def = m.GetGenericMethodDefinition();
                    var parameters = def.GetGenericArguments();
                    var newType = def.MakeGenericMethod(
                        args.Zip(parameters, (argument, parameter) =>
                            argument.DeclaringType == typeof(Generic) ? parameter : argument).ToArray()
                    );
                    return def;
                }
            }
            return m;
        }


        public static class Generic {
            public record T { }
            public enum Enum { Something }
            public record struct Struct { }
        }
    }
}
