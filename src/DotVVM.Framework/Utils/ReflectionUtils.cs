#nullable enable
using DotVVM.Framework.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Collections.Concurrent;
using DotVVM.Framework.Compilation.Binding;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Utils
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Gets the property name from lambda expression, e.g. 'a => a.FirstName'
        /// </summary>
        public static MemberInfo GetMemberFromExpression(Expression expression)
        {
            var originalExpression = expression;
            if (expression is LambdaExpression lambda)
                expression = lambda.Body;
            if (expression is UnaryExpression unary)
                expression = unary.Operand;

            var body = expression as MemberExpression;

            if (body == null)
                throw new NotSupportedException($"Can not get member from {originalExpression}");

            return body.Member;
        }

        // http://haacked.com/archive/2012/07/23/get-all-types-in-an-assembly.aspx/
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        ///<summary> Gets all members from the type, including inherited classes, implemented interfaces and interfaces inherited by the interface </summary>
        public static IEnumerable<MemberInfo> GetAllMembers(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            if (type.IsInterface)
                return type.GetMembers(flags).Concat(type.GetInterfaces().SelectMany(t => t.GetMembers(flags)));
            else
                return type.GetMembers(flags);
        }

        /// <summary>
        /// Gets filesystem path of assembly CodeBase
        /// http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
        /// </summary>
        public static string? GetCodeBasePath(this Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            if (codeBase == null) return null;
            UriBuilder uri = new UriBuilder(codeBase);
            return Uri.UnescapeDataString(uri.Path);
        }

        /// <summary>
        /// Gets the specified property of a given object.
        /// </summary>
        public static object? GetObjectPropertyValue(object? item, string propertyName, out PropertyInfo? prop)
        {
            prop = null;
            if (item == null) return null;

            var type = item.GetType();
            prop = type.GetProperty(propertyName);
            if (prop == null)
            {
                throw new Exception(String.Format("The object of type {0} does not have a property named {1}!", type, propertyName));     // TODO: exception handling
            }
            return prop.GetValue(item);
        }

        /// <summary>
        /// Checks whether given instantiated type is compatible with the open generic type
        /// </summary>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType, [NotNullWhen(returnValue: true)] out Type? commonType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                {
                    commonType = it;
                    return true;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                commonType = givenType;
                return true;
            }

            var baseType = givenType.BaseType;
            if (baseType == null)
            {
                commonType = null;
                return false;
            }

            return IsAssignableToGenericType(baseType, genericType, out commonType);
        }


        /// <summary>
        /// Converts a value to a specified type
        /// </summary>
        public static object? ConvertValue(object? value, Type type)
        {
            // handle null values
            if (value == null)
            {
                if (type.IsValueType)
                    return Activator.CreateInstance(type);
                else
                    return null;
            }

            // handle nullable types
            if (type.IsGenericType && Nullable.GetUnderlyingType(type) is Type nullableElementType)
            {
                if (value is string && (string)value == string.Empty)
                {
                    // value is an empty string, return null
                    return null;
                }

                // value is not null
                type = nullableElementType;
            }

            // handle exceptions
            if (value is string && type == typeof(Guid))
            {
                return new Guid((string)value);
            }
            if (type == typeof(object))
            {
                return value;
            }

            // handle enums
            if (type.IsEnum && value is string)
            {
                var split = ((string)value).Split(',', '|');
                var isFlags = type.IsDefined(typeof(FlagsAttribute));
                if (!isFlags && split.Length > 1) throw new Exception($"Enum {type} does allow multiple values. Use [FlagsAttribute] to allow it.");

                dynamic? result = null;
                foreach (var val in split)
                {
                    try
                    {
                        if (result == null) result = Enum.Parse(type, val.Trim(), ignoreCase: true); // Enum.TryParse requires type parameter
                        else
                        {
                            result |= (dynamic)Enum.Parse(type, val.Trim(), ignoreCase: true);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"The enum {type} does not allow a value '{val}'!", ex); // TODO: exception handling
                    }
                }
                return result;
            }

            // generic to string
            if (type == typeof(string))
            {
                return value.ToString();
            }

            // comma-separated array values
            if (value is string str && type.IsArray)
            {
                var objectArray = str.Split(',')
                    .Select(s => ConvertValue(s.Trim(), type.GetElementType()!))
                    .ToArray();
                var array = Array.CreateInstance(type.GetElementType()!, objectArray.Length);
                objectArray.CopyTo(array, 0);
                return array;
            }

            // numbers
            const NumberStyles numberStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;
            if (value is string str2)
            {
                if (type == typeof(double))
                    return double.Parse(str2, numberStyle & NumberStyles.Float, CultureInfo.InvariantCulture);
                else if (type == typeof(float))
                    return float.Parse(str2, numberStyle & NumberStyles.Float, CultureInfo.InvariantCulture);
                else if (type == typeof(decimal))
                    return decimal.Parse(str2, numberStyle & NumberStyles.Float, CultureInfo.InvariantCulture);
                else if (type == typeof(ulong))
                    return ulong.Parse(str2, numberStyle & NumberStyles.Integer, CultureInfo.InvariantCulture);
                else if (type.IsNumericType())
                    return Convert.ChangeType(long.Parse(str2, numberStyle & NumberStyles.Integer, CultureInfo.InvariantCulture), type, CultureInfo.InvariantCulture);
            }

            // convert
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        public static Type? GetEnumerableType(this Type collectionType)
        {
            var result = TypeDescriptorUtils.GetCollectionItemType(new ResolvedTypeDescriptor(collectionType));
            if (result == null) return null;
            return ResolvedTypeDescriptor.ToSystemType(result);
        }

        public static readonly HashSet<Type> DateTimeTypes = new HashSet<Type>()
        {
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan)
        };

        public static readonly HashSet<Type> NumericTypes = new HashSet<Type>()
        {
            typeof (sbyte),
            typeof (byte),
            typeof (short),
            typeof (ushort),
            typeof (int),
            typeof (uint),
            typeof (long),
            typeof (ulong),
            typeof (char),
            typeof (float),
            typeof (double),
            typeof (decimal)
        };

        public static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>() {
            typeof(string), typeof(char),
            typeof(bool),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
            typeof(Guid),
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        };

        public static bool IsNumericType(this Type type)
        {
            return NumericTypes.Contains(type);
        }

        public static bool IsDateOrTimeType(this Type type)
        {
            return DateTimeTypes.Contains(type);
        }

        /// <summary> Return true for Tuple, ValueTuple, KeyValuePair </summary>
        public static bool IsTupleLike(Type type) =>
            type.IsGenericType && (
            type.FullName!.StartsWith(typeof(Tuple).FullName + "`") ||
            type.FullName!.StartsWith(typeof(ValueTuple).FullName + "`") ||
            type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
            );

        public static bool IsEnumerable(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        public static bool IsDictionary(Type type)
        {
            return type.GetInterfaces().Any(x => x.IsGenericType
              && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }
        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentNullException($"'{genericInterfaceDefinition.FullName}' is not a generic interface definition.");
            }

            return type.GetInterfaces()
                .Concat(new[] { type })
                .Where(i => i.IsGenericType)
                .Any(i => i.GetGenericTypeDefinition() == genericInterfaceDefinition);
        }
        public static bool IsCollection(Type type)
        {
            return type != typeof(string) && IsEnumerable(type) && !IsDictionary(type);
        }

        public static bool IsPrimitiveType(Type type)
        {
            return PrimitiveTypes.Contains(type)
                || (IsNullableType(type) && IsPrimitiveType(type.UnwrapNullableType()))
                || type.IsEnum;
        }

        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsComplexType(Type type)
        {
            return !IsPrimitiveType(type);
        }

        public static bool IsDynamicOrObject(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider)) ||
                   type == typeof(object);
        }

        public static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type);
        }

        public static ParameterInfo[]? GetDelegateArguments(this Type type) =>
            type.IsDelegate() ?
            type.GetMethod("Invoke")!.GetParameters() :
            null;


        public static bool Implements(this Type type, Type ifc) => Implements(type, ifc, out var _);
        public static bool Implements(this Type type, Type ifc, out Type concreteInterface)
        {
            bool isInterface(Type a, Type b) => a == b || a.IsGenericType && a.GetGenericTypeDefinition() == b;
            if (isInterface(type, ifc))
            {
                concreteInterface = type;
                return true;
            }
            return (concreteInterface = type.GetInterfaces().FirstOrDefault(i => isInterface(i, ifc))) != null;
        }

        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
        public static Type UnwrapNullableType(this Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }
        public static Type MakeNullableType(this Type type)
        {
            return type.IsValueType && Nullable.GetUnderlyingType(type) == null && type != typeof(void) ? typeof(Nullable<>).MakeGenericType(type) : type;
        }


        public static T GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider, bool inherit = true) =>
            (T)attributeProvider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();

        public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider attributeProvider, bool inherit = true) =>
            attributeProvider.GetCustomAttributes(typeof(T), inherit).Cast<T>();


        private static ConcurrentDictionary<Type, string> cache_GetTypeHash = new ConcurrentDictionary<Type, string>();
        public static string GetTypeHash(this Type type)
        {
            return cache_GetTypeHash.GetOrAdd(type, t => {
                using (var sha1 = SHA1.Create())
                {
                    var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(t.AssemblyQualifiedName!));

                    return Convert.ToBase64String(hashBytes);
                }
            });
        }

        private static ConcurrentDictionary<Type, Func<Delegate, object?[], object?>> delegateInvokeCache = new ConcurrentDictionary<Type, Func<Delegate, object?[], object?>>();
        private static ParameterExpression delegateParameter = Expression.Parameter(typeof(Delegate), "delegate");
        private static ParameterExpression argsParameter = Expression.Parameter(typeof(object[]), "args");
        public static object? ExceptionSafeDynamicInvoke(this Delegate d, object?[] args) =>
            delegateInvokeCache.GetOrAdd(d.GetType(), type =>
                Expression.Lambda<Func<Delegate, object?[], object?>>(
                    Expression.Invoke(Expression.Convert(delegateParameter, type), d.Method.GetParameters().Select((p, i) =>
                        Expression.Convert(Expression.ArrayIndex(argsParameter, Expression.Constant(i)), p.ParameterType))).ConvertToObject(),
                delegateParameter, argsParameter)
                .Compile())
            .Invoke(d, args);

        public static Type GetResultType(this MemberInfo member) =>
            member is PropertyInfo property ? property.PropertyType :
            member is FieldInfo field ? field.FieldType :
            member is MethodInfo method ? method.ReturnType :
            member is TypeInfo type ? type.AsType() :
            throw new NotImplementedException($"Could not get return type of member {member.GetType().FullName}");
    }
}
