using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;

namespace DotVVM.Framework.Compilation.Binding
{
    public static class ExpressionHelper
    {
        public static Expression GetMember(Expression target, string name, Type[] typeArguments = null, bool throwExceptions = true)
        {
            Contract.Requires(target != null);

            if (target is MethodGroupExpression)
                throw new Exception("can not access member on method group");

            var type = target.Type;
            var isStatic = target is StaticClassIdentifierExpression;
            var isGeneric = typeArguments != null && typeArguments.Length != 0;
            var memberName = isGeneric ? $"{name}`{typeArguments.Length}" : name;

            var members = type.GetMembers(BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance))
                .Where(m => m.Name == memberName)
                .ToArray();
            if (members.Length == 0)
            {
                if (throwExceptions) throw new Exception($"could not find { (isStatic ? "static" : "instance") } member { memberName } on type { type.FullName }");
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
                    var nonGenericType = (Type)members[0];
                    return isGeneric 
                        ? Expression.Constant(null, nonGenericType)
                        : Expression.Constant(null, nonGenericType.MakeGenericType(typeArguments));
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
            // the following piece of code is nicer and more readable than method recognition done in roslyn, C# dynamic and also expression evaluator :)
            var method = FindValidMethodOveloads(target.Type, name, flags, typeArguments, arguments, namedArgs);
            return Expression.Call(target, method.Method, method.Arguments);
        }

        public static Expression CallMethod(Type target, BindingFlags flags, string name, Type[] typeArguments, Expression[] arguments, IDictionary<string, Expression> namedArgs = null)
        {
            // the following piece of code is nicer and more readable than method recognition done in roslyn, C# dynamic and also expression evaluator :)
            var method = FindValidMethodOveloads(target, name, flags, typeArguments, arguments, namedArgs);
            return Expression.Call(method.Method, method.Arguments);
        }


        private static MethodRecognitionResult FindValidMethodOveloads(Type type, string name, BindingFlags flags, Type[] typeArguments, Expression[] arguments, IDictionary<string, Expression> namedArgs)
        {
            var methods = FindValidMethodOveloads(type.GetMethods(flags).Where(m => m.Name == name), typeArguments, arguments, namedArgs);
            var method = methods.FirstOrDefault();
            if (method == null) throw new InvalidOperationException($"Could not find overload of method '{name}'.");
            return method;
        }

        private static IEnumerable<MethodRecognitionResult> FindValidMethodOveloads(IEnumerable<MethodInfo> methods, Type[] typeArguments, Expression[] arguments, IDictionary<string, Expression> namedArgs)
            => from m in methods
               let r = TryCallMethod(m, typeArguments, arguments, namedArgs)
               where r != null
               orderby r.CastCount descending, r.AutomaticTypeArgCount
               select r;

        class MethodRecognitionResult
        {
            public int AutomaticTypeArgCount { get; set; }
            public int CastCount { get; set; }
            public Expression[] Arguments { get; set; }
            public MethodInfo Method { get; set; }
        }

        private static MethodRecognitionResult TryCallMethod(MethodInfo method, Type[] typeArguments, Expression[] positionalArguments, IDictionary<string, Expression> namedArguments)
        {
            var parameters = method.GetParameters();

            int castCount = 0;
            if (parameters.Length < positionalArguments.Length) return null;
            var args = new Expression[parameters.Length];
            Array.Copy(positionalArguments, args, positionalArguments.Length);
            int namedArgCount = 0;
            for (int i = positionalArguments.Length; i < args.Length; i++)
            {
                if (namedArguments?.ContainsKey(parameters[i].Name) == true)
                {
                    args[i] = namedArguments[parameters[i].Name];
                    namedArgCount++;
                }
                else if (parameters[i].HasDefaultValue)
                {
                    castCount++;
                    args[i] = Expression.Constant(parameters[i].DefaultValue, parameters[i].ParameterType);
                }
                else return null;
            }

            // some named arguments were not used
            if (namedArguments != null && namedArgCount != namedArguments.Count) return null;

            int automaticTypeArgs = 0;
            // resolve generic parameters
            if (method.ContainsGenericParameters)
            {
                var typeArgs = new Type[method.GetGenericArguments().Length];
                if (typeArguments != null)
                {
                    if (typeArguments.Length > typeArgs.Length) return null;
                    Array.Copy(typeArguments, typeArgs, typeArgs.Length);
                }
                for (int i = 0; i < typeArgs.Length; i++)
                {
                    if (typeArgs[i] == null)
                    {
                        // try to resolve from arguments
                        var arg = Array.FindIndex(parameters, p => p.ParameterType.IsGenericParameter && p.ParameterType.GenericParameterPosition == i);
                        automaticTypeArgs++;
                        if (arg >= 0) typeArgs[i] = args[arg].Type;
                        else return null;
                    }
                }
                method = method.MakeGenericMethod(typeArgs);
                parameters = method.GetParameters();
            }
            else if (typeArguments != null) return null;

            // cast arguments
            for (int i = 0; i < args.Length; i++)
            {
                var casted = TypeConversion.ImplicitConversion(args[i], parameters[i].ParameterType);
                if (casted == null) return null;
                if (casted != args[i])
                {
                    castCount++;
                    args[i] = casted;
                }
            }

            return new MethodRecognitionResult
            {
                CastCount = castCount,
                AutomaticTypeArgCount = automaticTypeArgs,
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
            if (operation == ExpressionType.Assign) return Expression.Assign(left, TypeConversion.ImplicitConversion(right, left.Type, true, true));

            // TODO: type conversions
            if (operation == ExpressionType.AndAlso) return Expression.AndAlso(left, right);
            else if (operation == ExpressionType.OrElse) return Expression.OrElse(left, right);

            var binder = (DynamicMetaObjectBinder)Microsoft.CSharp.RuntimeBinder.Binder.BinaryOperation(
                CSharpBinderFlags.None, operation, typeof(object), GetBinderArguments(2));
            var result = ApplyBinder(binder, false, left, right);
            if (result != null) return result;
            if (operation == ExpressionType.Equal) return EqualsMethod(left, right);
            if (operation == ExpressionType.NotEqual) return Expression.Not(EqualsMethod(left, right));
            throw new Exception($"could not apply { operation } binary operator to { left } and { right }");
            // TODO: comparison operators
        }

        public static Expression GetUnaryOperator(Expression expr, ExpressionType operation)
        {
            var binder = (DynamicMetaObjectBinder)Microsoft.CSharp.RuntimeBinder.Binder.UnaryOperation(
                CSharpBinderFlags.None, operation, typeof(object), GetBinderArguments(1));
            return ApplyBinder(binder, true, expr);
        }

        public static Expression GetIndexer(Expression expr, Expression index)
        {
            if (expr.Type.IsArray) return Expression.ArrayIndex(expr, index);

            var indexProp = (from p in expr.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                             let param = p.GetIndexParameters()
                             where param.Length == 1
                             let indexConvert = TypeConversion.ImplicitConversion(index, param[0].ParameterType)
                             where indexConvert != null
                             select Expression.MakeIndex(expr, p, new[] { indexConvert })).ToArray();
            if (indexProp.Length == 0) throw new Exception($"could not find and indexer property on type { expr.Type } that accepts { index.Type } as argument");
            if (indexProp.Length > 1) throw new Exception($"more than one indexer found on type { expr.Type } that accepts { index.Type } as argument");
            return indexProp[0];
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

        private static Expression ApplyBinder(DynamicMetaObjectBinder binder, bool throwException, params Expression[] expressions)
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
            if (result.Expression.NodeType == ExpressionType.Throw)
            {
                if (throwException)
                {
                    // throw the exception
                    Expression.Lambda(result.Expression).Compile().DynamicInvoke();
                }
                else return null;
            }
            return result.Expression;
        }
    }
}
