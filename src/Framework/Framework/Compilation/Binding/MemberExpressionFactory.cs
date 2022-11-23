﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.Binding
{
    public class MemberExpressionFactory
    {
        private readonly IReadOnlyList<NamespaceImport> importedNamespaces;
        private readonly ExtensionMethodsCache extensionMethodsCache;
        private static readonly Type ParamArrayAttributeType = typeof(ParamArrayAttribute);

        public MemberExpressionFactory(ExtensionMethodsCache extensionMethodsCache, IReadOnlyList<NamespaceImport>? importedNamespaces = null)
        {
            this.extensionMethodsCache = extensionMethodsCache;
            this.importedNamespaces = importedNamespaces ?? ImmutableList<NamespaceImport>.Empty;
        }

        public Expression? GetMember(Expression target, string name, Type[]? typeArguments = null, bool throwExceptions = true, bool onlyMemberTypes = false, bool disableExtensionMethods = false)
        {
            if (target is MethodGroupExpression)
                throw new Exception("Cannot access member on method group.");

            var type = target.Type;
            if (type == typeof(UnknownTypeSentinel)) if (throwExceptions) throw new Exception($"Type of '{target}' could not be resolved."); else return null;

            var isStatic = target is StaticClassIdentifierExpression;

            var isGeneric = typeArguments != null && typeArguments.Length != 0;
            var genericName = isGeneric ? $"{name}`{typeArguments!.Length}" : name;

            if (!isGeneric && !onlyMemberTypes && typeof(DotvvmBindableObject).IsAssignableFrom(target.Type) &&
                GetDotvvmPropertyMember(target, name) is Expression result) return result;

            var bindingFlags = BindingFlags.Public | (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            // vast majority of member accesses are property access, so we'll just try type.GetProperty as it's
            // somewhat faster than getting all members. However, it may throw an exception if it's ambiguous, so we'll
            // swallow that and handle the error ourselves
            try
            {
                var p = type.GetProperty(name, bindingFlags);
                if (p is {})
                    return Expression.Property(isStatic ? null : target, p);
            }
            catch { }

            var members = new List<MemberInfo>();
            foreach (var m in type.GetAllMembers())
                if (((isGeneric && m is TypeInfo) ? genericName : name) == m.Name)
                    members.Add(m);

            var isExtension = false;
            if (members.Count == 0)
            {
                if (!disableExtensionMethods)
                {
                    // We did not find any match in regular methods => try extension methods
                    members = GetAllExtensionMethods().Where(m => m.Name == name && ExtensionMethodsFilter(target, m)).ToList<MemberInfo>();
                    isExtension = true;
                }

                if (members.Count == 0 && throwExceptions)
                    throw new Exception($"Could not find { (isStatic ? "static" : "instance") } member { name } on type { type.FullName }.");
                else if (members.Count == 0 && !throwExceptions)
                    return null;
            }
            if (members.Count == 1)
            {
                if (members[0] is not Type && onlyMemberTypes) { throw new Exception("Only type names are supported."); }

                var instance = isStatic ? null : target;
                if (members[0] is PropertyInfo property)
                {
                    return Expression.Property(instance, property);
                }
                else if (members[0] is FieldInfo field)
                {
                    if (field.IsLiteral)
                        return Expression.Constant(field.GetValue(null), field.FieldType);
                    else
                        return Expression.Field(instance, field);
                }
                else if (members[0] is Type nonGenericType)
                {
                    return isGeneric
                        ? new StaticClassIdentifierExpression(nonGenericType.MakeGenericType(typeArguments!))
                        : new StaticClassIdentifierExpression(nonGenericType.UnderlyingSystemType);
                }
            }

            var candidates = members.Cast<MethodInfo>().ToList();
            return new MethodGroupExpression(target, name, typeArguments, candidates, isExtension);
        }

        private bool ExtensionMethodsFilter(Expression target, MethodInfo method)
        {
            var thisType = method.GetParameters().First().ParameterType;
            if (thisType.IsGenericType)
            {
                if (thisType.ContainsGenericParameters)
                {
                    return ReflectionUtils.IsAssignableToGenericType(target.Type, thisType.GetGenericTypeDefinition(), out _);
                }
                else
                {
                    return thisType.IsAssignableFrom(target.Type);
                }
            }
            else
            {
                return thisType.IsAssignableFrom(target.Type);
            }
        }

        private Expression? GetDotvvmPropertyMember(Expression target, string name)
        {
            var property = DotvvmProperty.ResolveProperty(target.Type, name);
            if (property == null) return null;

            return Expression.Convert(
                Expression.Call(target, "GetValue", Type.EmptyTypes,
                    Expression.Constant(property),
                    Expression.Constant(true)
                ),
                property.PropertyType
            );
        }

        /// <summary>
        /// Creates an expression that updates the member inside <paramref name="leftWrapped"/> with a
        /// new <paramref name="value"/>.
        /// </summary>
        /// <remarks>
        /// Should <paramref name="leftWrapped"/> contain a call to the
        /// <see cref="DotvvmBindableObject.GetValue(DotvvmProperty, bool)"/> method, it will be
        /// replaced with a <see cref="DotvvmBindableObject.SetValue(DotvvmProperty, object)"/>
        /// call.
        /// </remarks>
        public Expression? UpdateMember(Expression leftWrapped, Expression value)
        {
            var left = leftWrapped;
            while (left.NodeType == ExpressionType.Convert
                && left is UnaryExpression unary)
            {
                left = unary.Operand;
            }

            if (left is IndexExpression indexExpression)
            {
                // Convert to explicit method call `set_{Indexer}(index, value)`
                var setMethod = indexExpression.Indexer?.SetMethod;
                if (setMethod is null)
                    throw new Exception($"Can not set to {indexExpression}, the indexer does not have a setter.");
                return Expression.Call(indexExpression.Object, setMethod, indexExpression.Arguments.Concat(new[] { value }));
            }
            else if (left.NodeType == ExpressionType.ArrayIndex && left is BinaryExpression arrayIndexExpression)
            {
                // Convert to explicit method call `Array.SetValue(value, index)`
                var setMethod = typeof(Array).GetMethod(nameof(Array.SetValue), BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(object), typeof(int) }, null)!;
                // If we are working with array of value types then box the value
                var valueAsObj = (value != null && value.Type.IsValueType) ? Expression.TypeAs(value, typeof(object)) : value!;
                return Expression.Call(arrayIndexExpression.Left, setMethod, new Expression[] { valueAsObj, arrayIndexExpression.Right /* index */ });
            }


            if ((left.NodeType == ExpressionType.MemberAccess
                && left is MemberExpression member
                && member.Member is PropertyInfo property
                && property.CanWrite)
                || left.NodeType == ExpressionType.Parameter)
            {
                return Expression.Assign(leftWrapped, Expression.Convert(value, leftWrapped.Type));
            }


            if (left.NodeType == ExpressionType.Call
                && left is MethodCallExpression call
                && call.Method.DeclaringType == typeof(DotvvmBindableObject)
                && call.Method.Name == nameof(DotvvmBindableObject.GetValue)
                && call.Arguments.Count == 2
                && call.Arguments[0].Type == typeof(DotvvmProperty)
                && call.Arguments[1].Type == typeof(bool))
            {
                var propertyArgument = call.Arguments[0];
                var setValue = typeof(DotvvmBindableObject)
                    .GetMethod(nameof(DotvvmBindableObject.SetValueToSource),
                        new[] { typeof(DotvvmProperty), typeof(object) })!;
                return Expression.Call(call.Object, setValue, propertyArgument, Expression.Convert(value, typeof(object)));
            }

            return null;
        }

        public Expression Call(Expression target, Expression[] arguments)
        {
            if (target is MethodGroupExpression methodGroup)
            {
                return methodGroup.CreateMethodCall(arguments, this);
            }
            return Expression.Invoke(target, arguments);
        }

        public Expression CallMethod(Expression target, BindingFlags flags, string name, Type[]? typeArguments, Expression[] arguments, IDictionary<string, Expression>? namedArgs = null)
        {
            // the following piece of code is nicer and more readable than method recognition done in roslyn, C# dynamic and also expression evaluator :)
            var method = FindValidMethodOverloads(target, target.Type, name, flags, typeArguments, arguments, namedArgs);

            if (method.IsExtension)
            {
                // Change to a static call
                return Expression.Call(method.Method, method.Arguments);
            }
            return Expression.Call(target, method.Method, method.Arguments);
        }

        public Expression CallMethod(Type target, BindingFlags flags, string name, Type[]? typeArguments, Expression[] arguments, IDictionary<string, Expression>? namedArgs = null)
        {
            // the following piece of code is nicer and more readable than method recognition done in roslyn, C# dynamic and also expression evaluator :)
            var method = FindValidMethodOverloads(null, target, name, flags, typeArguments, arguments, namedArgs);
            return Expression.Call(method.Method, method.Arguments);
        }


        private MethodRecognitionResult FindValidMethodOverloads(Expression? target, Type type, string name, BindingFlags flags, Type[]? typeArguments, Expression[] arguments, IDictionary<string, Expression>? namedArgs)
        {
            var methods = FindValidMethodOverloads(type.GetAllMethods(flags), name, false, typeArguments, arguments, namedArgs);

            if (methods.Count == 1) return methods[0];
            if (methods.Count == 0)
            {
                // We did not find any match in regular methods => try extension methods
                if (target != null)
                {
                    // Change to a static call
                    var newArguments = new[] { target }.Concat(arguments).ToArray();
                    var extensions = FindValidMethodOverloads(GetAllExtensionMethods(), name, true, typeArguments, newArguments, namedArgs);

                    // We found an extension method
                    if (extensions.Count == 1)
                        return extensions[0];

                    target = null;
                    methods = extensions;
                    arguments = newArguments;
                }

                if (methods.Count == 0)
                    throw new InvalidOperationException($"Could not find method overload nor extension method that matched '{name}'.");
            }

            // There are multiple method candidates
            methods = methods.OrderBy(s => s.CastCount).ThenBy(s => s.AutomaticTypeArgCount).ThenBy(s => s.HasParamsAttribute).ToList();
            var method = methods.First();
            var method2 = methods.Skip(1).First();
            if (method.AutomaticTypeArgCount == method2.AutomaticTypeArgCount && method.CastCount == method2.CastCount && method.HasParamsAttribute == method2.HasParamsAttribute)
            {
                // TODO: this behavior is not completed. Implement the same behavior as in roslyn.
                var foundOverloads = $"{method.Method}, {method2.Method}";
                throw new InvalidOperationException($"Found ambiguous overloads of method '{name}'. The following overloads were found: {foundOverloads}.");
            }
            return method;
        }

        private IEnumerable<MethodInfo> GetAllExtensionMethods()
        {
            return extensionMethodsCache.GetExtensionsForNamespaces(importedNamespaces.Select(ns => ns.Namespace).Distinct().ToArray());
        }

        private List<MethodRecognitionResult> FindValidMethodOverloads(IEnumerable<MethodInfo> methods, string name, bool isExtension, Type[]? typeArguments, Expression[] arguments, IDictionary<string, Expression>? namedArgs)
        {
            var result = new List<MethodRecognitionResult>();
            foreach (var m in methods)
            {
                if (m is null || m.Name != name)
                    continue;
                var r = TryCallMethod(m, typeArguments, arguments, namedArgs);
                if (r is {})
                {
                    r.IsExtension = isExtension;
                    result.Add(r);
                }
            }
            return result;
        }

        sealed class MethodRecognitionResult
        {
            public MethodRecognitionResult(int automaticTypeArgCount, int castCount, Expression[] arguments, MethodInfo method, int paramsArrayCount, bool isExtension, bool hasParamsAttribute)
            {
                AutomaticTypeArgCount = automaticTypeArgCount;
                CastCount = castCount;
                Arguments = arguments;
                Method = method;
                ParamsArrayCount = paramsArrayCount;
                IsExtension = isExtension;
                HasParamsAttribute = hasParamsAttribute;
            }

            public int AutomaticTypeArgCount { get; set; }
            public int CastCount { get; set; }
            public Expression[] Arguments { get; set; }
            public MethodInfo Method { get; set; }
            public int ParamsArrayCount { get; set; }
            public bool IsExtension { get; set; }
            public bool HasParamsAttribute { get; set; }
        }

        private MethodRecognitionResult? TryCallMethod(MethodInfo method, Type[]? typeArguments, Expression[] positionalArguments, IDictionary<string, Expression>? namedArguments)
        {
            var parameters = method.GetParameters();

            if (!TryPrepareArguments(parameters, positionalArguments, namedArguments, out var args, out var castCount))
                return null;

            var hasParamsArrayAttributes = parameters.LastOrDefault()?.IsDefined(ParamArrayAttributeType) == true;
            int automaticTypeArgs = 0;
            // resolve generic parameters
            if (method.ContainsGenericParameters)
            {
                var genericArguments = method.GetGenericArguments();
                var typeArgs = new Type[genericArguments.Length];
                if (typeArguments != null)
                {
                    if (typeArguments.Length > typeArgs.Length) return null;
                    Array.Copy(typeArguments, typeArgs, typeArgs.Length);
                }
                var parameterTypes = parameters.Select(s => s.ParameterType).ToArray();
                if (hasParamsArrayAttributes && parameterTypes.Length > 0)
                {
                    parameterTypes[parameterTypes.Length - 1] = parameterTypes.Last().GetElementType().NotNull();
                }
                for (int genericArgumentPosition = 0; genericArgumentPosition < typeArgs.Length; genericArgumentPosition++)
                {
                    if (typeArgs[genericArgumentPosition] == null)
                    {
                        // try to resolve from arguments
                        var argType = GetGenericParameterType(genericArguments[genericArgumentPosition], parameterTypes, args.Select(s => s.Type).ToArray());
                        automaticTypeArgs++;
                        if (argType != null) typeArgs[genericArgumentPosition] = argType;
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
                Type elm;
                if (args.Length == i + 1 && hasParamsArrayAttributes && !args[i].Type.IsArray)
                {
                    elm = parameters[i].ParameterType.GetElementType().NotNull();
                    if (positionalArguments.Skip(i).Any(s => TypeConversion.ImplicitConversion(s, elm) is null))
                    {
                        return null;
                    }
                }
                else
                {
                    elm = parameters[i].ParameterType;
                }
                var casted = TypeConversion.ImplicitConversion(args[i], elm);
                if (casted == null)
                {
                    return null;
                }
                if (casted != args[i])
                {
                    castCount++;
                    args[i] = casted;
                }
                if (args.Length == i + 1 && hasParamsArrayAttributes && !args[i].Type.IsArray)
                {
                    var converted = positionalArguments.Skip(i)
                        .Select(a => TypeConversion.ImplicitConversion(a, elm, throwException: true)!)
                        .ToArray();
                    args[i] = NewArrayExpression.NewArrayInit(elm, converted);
                }
            }

            return new MethodRecognitionResult(
                automaticTypeArgCount: automaticTypeArgs,
                castCount: castCount,
                method: method,
                arguments: args,
                paramsArrayCount: positionalArguments.Length - args.Length,
                hasParamsAttribute: hasParamsArrayAttributes,
                isExtension: false
            );
        }
        private static bool TryPrepareArguments(ParameterInfo[] parameters, Expression[] positionalArguments, IDictionary<string, Expression>? namedArguments, [MaybeNullWhen(false)] out Expression[] arguments, out int castCount)
        {
            castCount = 0;
            arguments = null;
            var addedArguments = 0;
            var hasParamsArrayAttribute = parameters.LastOrDefault()?.IsDefined(ParamArrayAttributeType) == true;

            // For methods without `params` arguments count must be at least equal to parameters count
            if (!hasParamsArrayAttribute && parameters.Length < positionalArguments.Length)
                return false;

            arguments = new Expression[parameters.Length];
            var copyItemsCount = !hasParamsArrayAttribute ? positionalArguments.Length : parameters.Length;

            if (hasParamsArrayAttribute && parameters.Length > positionalArguments.Length)
            {
                var parameter = parameters.Last();
                var elementType = parameter.ParameterType.GetElementType().NotNull();

                // User specified no arguments for the `params` array, we need to create an empty array
                arguments[arguments.Length - 1] = Expression.NewArrayInit(elementType);

                // Last argument was just generated => do not copy
                addedArguments++;
                copyItemsCount--;
            }
            if (copyItemsCount > positionalArguments.Length)
            {
                // Check if we could use default parameters
                var defaultParametersCount = parameters.Skip(positionalArguments.Length).Where(param => param.HasDefaultValue).Count();
                if (defaultParametersCount + positionalArguments.Length >= copyItemsCount)
                    copyItemsCount = positionalArguments.Length;
                else
                    return false;
            }

            Array.Copy(positionalArguments, arguments, copyItemsCount);

            // Process named arguments
            var namedArgCount = 0;
            for (var i = positionalArguments.Length; i < arguments.Length; i++)
            {
                if (namedArguments?.ContainsKey(parameters[i].Name!) == true)
                {
                    arguments[i] = namedArguments[parameters[i].Name!];
                    namedArgCount++;
                }
                else if (parameters[i].HasDefaultValue)
                {
                    castCount++;
                    arguments[i] = Expression.Constant(parameters[i].DefaultValue, parameters[i].ParameterType);
                }
                else if (parameters[i].IsDefined(ParamArrayAttributeType))
                {
                    break;
                }
                else return false;
            }

            // Some named arguments were not used
            if (namedArguments != null && namedArgCount != namedArguments.Count)
                return false;

            return true;
        }

        private Type? GetGenericParameterType(Type genericArg, Type[] searchedGenericTypes, Type[] expressionTypes)
        {
            for (var i = 0; i < searchedGenericTypes.Length; i++)
            {
                if (expressionTypes.Length <= i) return null;
                var sgt = searchedGenericTypes[i];
                if (sgt == genericArg)
                {
                    return expressionTypes[i];
                }
                if (sgt.IsArray)
                {
                    var elementType = sgt.GetElementType();
                    var expressionElementType = expressionTypes[i].GetElementType();
                    if (elementType == genericArg)
                        return expressionElementType;
                    else
                        return GetGenericParameterType(genericArg, searchedGenericTypes[i].GetGenericArguments(), expressionTypes[i].GetGenericArguments());
                }
                else if (sgt.IsGenericType)
                {
                    Type[]? genericArguments = null;
                    var expression = expressionTypes[i];

                    if (expression.IsArray)
                    {
                        // Arrays need to be handled in a special way to obtain instantiation
                        genericArguments = new[] { expression.GetElementType().NotNull() };
                    }
                    else
                    {
                        if (expression.IsGenericType && sgt.GetGenericTypeDefinition() == expression.GetGenericTypeDefinition())
                        {
                            // We have exactly the same type => return generic arguments
                            genericArguments = expression.GetGenericArguments();
                        }
                        else if (sgt.IsInterface)
                        {
                            // We must find the instantiation within an implemented generic interface
                            var implementation = expression.GetInterfaces().Where(ifc => ifc.IsGenericType && ifc.GetGenericTypeDefinition() == sgt.GetGenericTypeDefinition()).Take(2).ToList();
                            if (implementation.Count == 1)
                            {
                                genericArguments = implementation.Single().GetGenericArguments();
                            }
                        }
                        else
                        {
                            // Otherwise we must find the instantiation within a generic base type
                            genericArguments = null;
                            var current = expression.BaseType;
                            while (current != null)
                            {
                                if (current.IsGenericType && current.GetGenericTypeDefinition() == sgt.GetGenericTypeDefinition())
                                {
                                    genericArguments = current.GetGenericArguments();
                                    break;
                                }

                                current = current.BaseType;
                            }
                        }
                    }

                    if (genericArguments != null)
                    {
                        var value = GetGenericParameterType(genericArg, sgt.GetGenericArguments(), genericArguments);
                        if (value is Type) return value;
                    }
                }
            }
            return null;
        }

        public Expression EqualsMethod(Expression left, Expression right)
        {
            Expression? equatable = null;
            Expression? theOther = null;
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
                var m = CallMethod(equatable, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Equals", null, new[] { theOther! });
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
                theOther = TypeConversion.ImplicitConversion(theOther!, equatable.Type);
                if (theOther != null) return Expression.Equal(equatable, theOther);
            }

            return CallMethod(left, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Equals", null, new[] { right });
        }

        public Expression CompareMethod(Expression left, Expression right)
        {
            Type compareType = typeof(object);
            Expression? equatable = null;
            Expression? theOther = null;
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
                return CallMethod(equatable, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy, "Compare", null, new[] { theOther! });
            }
            throw new NotSupportedException("IComparable is not implemented on any of specified types");
        }

        public Expression GetUnaryOperator(Expression expr, ExpressionType operation)
        {
            var binder = (DynamicMetaObjectBinder)Microsoft.CSharp.RuntimeBinder.Binder.UnaryOperation(
                CSharpBinderFlags.None, operation, typeof(object), ExpressionHelper.GetBinderArguments(1));
            return ExpressionHelper.ApplyBinder(binder, true, expr)!;
        }

        public Expression GetBinaryOperator(Expression left, Expression right, ExpressionType operation)
        {
            if (operation == ExpressionType.Coalesce) return Expression.Coalesce(left, right);
            if (operation == ExpressionType.Assign)
            {
                return UpdateMember(left, TypeConversion.ImplicitConversion(right, left.Type, true, true)!)
                    .NotNull($"Expression '{right}' cannot be assigned into '{left}'.");
            }

            // TODO: type conversions
            if (operation == ExpressionType.AndAlso) return Expression.AndAlso(left, right);
            else if (operation == ExpressionType.OrElse) return Expression.OrElse(left, right);

            var binder = (DynamicMetaObjectBinder)Microsoft.CSharp.RuntimeBinder.Binder.BinaryOperation(
                CSharpBinderFlags.None, operation, typeof(object), ExpressionHelper.GetBinderArguments(2));
            var result = ExpressionHelper.ApplyBinder(binder, false, left, right);
            if (result != null) return result;
            if (operation == ExpressionType.Equal) return EqualsMethod(left, right);
            if (operation == ExpressionType.NotEqual) return Expression.Not(EqualsMethod(left, right));


            // try converting left to right.Type and vice versa
            // needed to enum with pseudo-string literal operations
            // if (TypeConversion.ImplicitConversion(left, right.Type) is {} leftConverted)
            //     return GetBinaryOperator(leftConverted, right, operation);
            // if (TypeConversion.ImplicitConversion(right, left.Type) is {} rightConverted)
            //     return GetBinaryOperator(left, rightConverted, operation);

            // lift the operator
            if (left.Type.IsNullable() || right.Type.IsNullable())
                return GetBinaryOperator(left.UnwrapNullable(), right.UnwrapNullable(), operation);

            throw new Exception($"could not apply { operation } binary operator to { left } and { right }");
            // TODO: comparison operators
        }
    }
}
