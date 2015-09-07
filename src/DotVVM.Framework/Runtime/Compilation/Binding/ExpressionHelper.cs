using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public static class ExpressionHelper
    {
        public static Expression GetMember(Expression target, string name, Type[] typeArguments = null, bool throwExceptions = true)
        {
            if (target is MethodGroupExpression)
                throw new Exception("can't access member on method group");

            var type = target.Type;
            var isStatic = target is StaticClassIdentifierExpression;
            var members = type.GetMembers(BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance))
                .Where(m => m.Name == name)
                .ToArray();
            if (members.Length == 0)
            {
                if (throwExceptions) throw new Exception($"couldn't find { (isStatic ? "static" : "instance") } member { name } on type { type.FullName }");
                else return null;
            }
            if (members.Length == 1)
            {
                var instance = isStatic ? null : target;
                if (members[0] is PropertyInfo)
                {
                    var property = members[0] as PropertyInfo;
                    return Expression.Property(instance, property);
                }
                else if (members[0] is FieldInfo)
                {
                    var field = members[0] as FieldInfo;
                    return Expression.Field(instance, field);
                }
                else if (members[0] is Type)
                {
                    return Expression.Constant(null, (Type)members[0]);
                }
            }
            return new MethodGroupExpression() { MethodName = name, Target = target, TypeArgs = typeArguments };
        }

        public static Expression Call(Expression target, Expression[] arguments)
        {
            if (target is MethodGroupExpression)
            {
                return ((MethodGroupExpression)target).CreateMethodCall(arguments);
            }
            return Expression.Invoke(target, arguments);
        }

        public static Expression CallMethod(Expression target, BindingFlags flags, string name, Type[] typeArguments, Expression[] arguments, IDictionary<string, Expression> namedArgs = null)
        {
            // the following piece of code is nicer and more readable than method recognition done in roslyn, c# dynamic and also expression evaluator :)
            var methods = from m in target.Type.GetMethods(flags)
                          where m.Name == name
                          let r = TryCallMethod(target, m, typeArguments, arguments, namedArgs)
                          orderby r.CastCount descending
                          select r;
            var method = methods.First();
            return Expression.Call(target, method.Method, method.Arguments);
        }

        class MethodRecognitionResult
        {
            public int CastCount { get; set; }
            public Expression[] Arguments { get; set; }
            public MethodInfo Method { get; set; }
        }

        private static MethodRecognitionResult TryCallMethod(Expression target, MethodInfo method, Type[] typeArguments, Expression[] positionalArguments, IDictionary<string, Expression> namedArguments)
        {
            var parameters = method.GetParameters();
            // method must have all named arguments
            if (namedArguments != null && !namedArguments.All(n => parameters.Any(p => p.Name == n.Key))) return null;

            int weight = 0;
            var args = new Expression[parameters.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (namedArguments?.ContainsKey(parameters[i].Name) == true)
                {
                    args[i] = namedArguments[parameters[i].Name];
                }
                else if (i < positionalArguments.Length)
                {
                    args[i] = positionalArguments[i];
                }
                else if (parameters[i].HasDefaultValue)
                {
                    weight++;
                    args[i] = Expression.Constant(parameters[i].DefaultValue, parameters[i].ParameterType);
                }
                else return null;
            }

            // resolve generic parameters
            if (method.ContainsGenericParameters)
            {
                var typeArgs = new Type[method.GetGenericArguments().Length];
                if (typeArguments != null) Array.Copy(typeArguments, typeArgs, typeArgs.Length);
                for (int i = 0; i < typeArgs.Length; i++)
                {
                    if (typeArgs[i] == null)
                    {
                        // try to resolve from arguments
                        var arg = Array.FindIndex(parameters, p => p.ParameterType.IsGenericParameter && p.ParameterType.GenericParameterPosition == i);
                        if (arg >= 0) typeArgs[i] = args[arg].Type;
                        else return null;
                    }
                }
                method = method.MakeGenericMethod(typeArgs);
                parameters = method.GetParameters();
            }

            // cast arguments
            for (int i = 0; i < args.Length; i++)
            {
                var casted = TypeConversion.ImplicitConversion(args[i], parameters[i].ParameterType);
                if (casted == null) return null;
                if(casted != args[i])
                {
                    weight++;
                    args[i] = casted;
                }
            }

            return new MethodRecognitionResult
            {
                CastCount = weight,
                Method = method,
                Arguments = args
            };
        }


        public static Expression EqualsMethod(Expression left, Expression right)
        {
            Expression equatable = null;
            Expression theOther = null;
            if (typeof(IEquatable<>).IsAssignableFrom(left.Type))
            {
                equatable = left;
                theOther = right;
            }
            else if (typeof(IEquatable<>).IsAssignableFrom(right.Type))
            {
                equatable = right;
                theOther = left;
            }

            if (equatable != null)
            {
                var m = CallMethod(equatable, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Equals", null, new[] { theOther });
                if (m != null) return m;
            }

            if (left.Type.IsValueType)
            {
                equatable = left;
                theOther = right;
            }
            else if (left.Type.IsValueType)
            {
                equatable = right;
                theOther = left;
            }

            if (equatable != null)
            {
                theOther = TypeConversion.ImplicitConversion(theOther, equatable.Type);
                if (theOther != null) return Expression.Equal(equatable, theOther);
            }

            return CallMethod(left, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Equals", null, new[] { right });
        }

        public static Expression CompareMethod(Expression left, Expression right)
        {
            Type compareType = typeof(object);
            Expression equatable = null;
            Expression theOther = null;
            if (typeof(IComparable<>).IsAssignableFrom(left.Type))
            {
                equatable = left;
                theOther = right;
            }
            else if (typeof(IComparable<>).IsAssignableFrom(right.Type))
            {
                equatable = right;
                theOther = left;
            }
            else if (typeof(IComparable).IsAssignableFrom(left.Type))
            {
                equatable = left;
                theOther = right;
            }
            else if (typeof(IComparable).IsAssignableFrom(right.Type))
            {
                equatable = right;
                theOther = left;
            }

            if (equatable != null)
            {
                return CallMethod(equatable, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Compare", null, new[] { theOther });
            }
            throw new NotSupportedException("IComparable is not implemented on any of specified types");
        }

        public static Expression GetBinaryOperator(Expression left, Expression right, ExpressionType operation)
        {
            if (operation == ExpressionType.Coalesce) return Expression.Coalesce(left, right);

            // TODO: support lazy evaluation
            if (operation == ExpressionType.AndAlso) operation = ExpressionType.And;
            else if (operation == ExpressionType.OrElse) operation = ExpressionType.Or;

            var binder = (DynamicMetaObjectBinder)Microsoft.CSharp.RuntimeBinder.Binder.BinaryOperation(
                CSharpBinderFlags.None, operation, typeof(object), GetBinderArguments(2));
            return ApplyBinder(binder, left, right) ??
                (operation == ExpressionType.Equal ? EqualsMethod(left, right) : null) ??
                (operation == ExpressionType.NotEqual ? Expression.Not(EqualsMethod(left, right)) : null);
            // TODO: comparison operators
        }

        public static Expression GetUnaryOperator(Expression expr, ExpressionType operation)
        {
            var binder = (DynamicMetaObjectBinder)Microsoft.CSharp.RuntimeBinder.Binder.UnaryOperation(
                CSharpBinderFlags.None, operation, typeof(object), GetBinderArguments(1));
            return ApplyBinder(binder, expr);
        }

        public static Expression GetIndexer(Expression expr, Expression index)
        {
            var binder = (DynamicMetaObjectBinder)Microsoft.CSharp.RuntimeBinder.Binder.GetIndex(
                CSharpBinderFlags.None, typeof(object), GetBinderArguments(2));
            return ApplyBinder(binder, expr, index);
        }

        private static IEnumerable<CSharpArgumentInfo> GetBinderArguments(int count)
        {
            var arr = new CSharpArgumentInfo[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null);
            }
            return arr;
        }

        private static Expression ApplyBinder(DynamicMetaObjectBinder binder, params Expression[] expressions)
        {
            var result = binder.Bind(DynamicMetaObject.Create(null, expressions[0]),
                expressions.Skip(1).Select(e =>
                    DynamicMetaObject.Create(null, e)).ToArray()
            );

            if (result.Expression.NodeType == ExpressionType.Convert)
            {
                var convert = (UnaryExpression)result.Expression;
                return convert.Operand;
            }
            if (result.Expression.NodeType == ExpressionType.Throw) return null;
            return result.Expression;
        }
    }
}
