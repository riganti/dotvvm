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
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using FastExpressionCompiler;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RecordExceptions;

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
            if (expression.NodeType == ExpressionType.Lambda)
                expression = ((LambdaExpression)expression).Body;

            while (expression is UnaryExpression unary)
                expression = unary.Operand;

            var body = expression as MemberExpression;

            if (body == null)
                throw new NotSupportedException($"Cannot get member from {originalExpression}");

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
                return e.Types.Where(t => t != null)!;
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

        ///<summary> Gets all methods from the type, including inherited classes, implemented interfaces and interfaces inherited by the interface </summary>
        public static IEnumerable<MethodInfo> GetAllMethods(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            if (type.IsInterface)
                return type.GetMethods(flags).Concat(type.GetInterfaces().SelectMany(t => t.GetMethods(flags)));
            else
                return type.GetMethods(flags);
        }

        /// <summary>
        /// Gets filesystem path of assembly CodeBase
        /// http://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
        /// </summary>
        public static string? GetCodeBasePath(this Assembly assembly)
        {
            return assembly.Location;
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
        /// <exception cref="TypeConvertException" />
        public static object? ConvertValue(object? value, Type type)
        {
            // handle null values
            if (value == null)
            {
                if (type == typeof(bool))
                    return BoxingUtils.False;
                else if (type == typeof(int))
                    return BoxingUtils.Zero;
                else if (type.IsValueType)
                    return Activator.CreateInstance(type);
                else
                    return null;
            }

            if (type.IsInstanceOfType(value)) return value;

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
                        throw new Exception($"The enum {type} does not allow a value '{val}'!", ex);
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
                if (type == typeof(bool))
                    return BoxingUtils.Box(bool.Parse(str2));
                else if (type == typeof(int))
                    return BoxingUtils.Box(int.Parse(str2, numberStyle & NumberStyles.Integer, CultureInfo.InvariantCulture));
                else if (type == typeof(double))
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
            try
            {
                return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new TypeConvertException(value, type, e);
            }
        }

        public record TypeConvertException(object Value, Type Type, Exception InnerException): RecordException(InnerException)
        {
            public override string Message => $"Can not convert value '{Value}' to {Type}: {InnerException!.Message}";
        }

        public static Type? GetEnumerableType(this Type collectionType)
        {
            var result = TypeDescriptorUtils.GetCollectionItemType(new ResolvedTypeDescriptor(collectionType));
            if (result == null) return null;
            return ResolvedTypeDescriptor.ToSystemType(result);
        }

        private static readonly HashSet<Type> DateTimeTypes = new HashSet<Type>()
        {
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DateOnly),
            typeof(TimeOnly)
        };

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>()
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
        private static readonly HashSet<Type> IntegerNumericTypes = new HashSet<Type>()
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
        };
        private static readonly HashSet<Type> RealNumericTypes = new HashSet<Type>()
        {
            typeof (float),
            typeof (double),
            typeof (decimal)
        };

        public static IEnumerable<Type> GetNumericTypes()
        {
            return NumericTypes;
        }

        private static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>() {
            typeof(string), typeof(char),
            typeof(bool),
            typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(DateOnly), typeof(TimeOnly),
            typeof(Guid),
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        };

        public static bool IsNumericType(this Type type) => NumericTypes.Contains(type);
        public static bool IsDateOrTimeType(this Type type) => DateTimeTypes.Contains(type);
        public static bool IsIntegerNumericType(this Type type) => IntegerNumericTypes.Contains(type);
        public static bool IsRealNumericType(this Type type) => RealNumericTypes.Contains(type);

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

        public static bool IsSerializationSupported(this Type type, bool includeNullables)
        {
            if (includeNullables)
                return IsPrimitiveType(type);

            return PrimitiveTypes.Contains(type);
        }

        public static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsComplexType(Type type)
        {
            return !IsPrimitiveType(type);
        }

        public static bool IsDelegate(this Type type) =>
            typeof(Delegate).IsAssignableFrom(type);
        public static bool IsDelegate(this Type type, [NotNullWhen(true)] out MethodInfo? invokeMethod)
        {
            if (type.IsDelegate() && typeof(Delegate) != type)
            {
                invokeMethod = type.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance).NotNull("Could not find delegate Invoke method");
                return true;
            }
            else
            {
                invokeMethod = null;
                return false;
            }
        }

        public static ParameterInfo[]? GetDelegateArguments(this Type type) =>
            type.IsDelegate(out var m) ? m.GetParameters() : null;


        public static bool Implements(this Type type, Type ifc) => Implements(type, ifc, out var _);
        public static bool Implements(this Type type, Type ifc, [NotNullWhen(true)] out Type? concreteInterface)
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

        public static Type UnwrapTaskType(this Type type)
        {
            if (type.IsGenericType && typeof(Task<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                return type.GetGenericArguments()[0];
            else if (typeof(Task).IsAssignableFrom(type))
                return typeof(void);
            else if (type.IsGenericType && typeof(ValueTask<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                return type.GetGenericArguments()[0];
            else
                return type;
        }

        public static bool IsValueOrBinding(this Type type, [NotNullWhen(true)] out Type? elementType)
        {
            type = type.UnwrapNullableType();
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueOrBinding<>))
            {
                elementType = type.GenericTypeArguments.Single();
                return true;
            }
            else if (typeof(ValueOrBinding).IsAssignableFrom(type))
            {
                elementType = typeof(object);
                return true;
            }
            else
            {
                elementType = null;
                return false;
            }
        }

        public static Type UnwrapValueOrBinding(this Type type) =>
            type.IsValueOrBinding(out var x) ? x : type;

        public static T? GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider, bool inherit = true) =>
            (T?)attributeProvider.GetCustomAttributes(typeof(T), inherit).FirstOrDefault();

        public static T[] GetCustomAttributes<T>(this ICustomAttributeProvider attributeProvider, bool inherit = true)
        {
            var resultObj = attributeProvider.GetCustomAttributes(typeof(T), inherit);
            var resultT = new T[resultObj.Length];
            for (int i = 0; i < resultObj.Length; i++)
            {
                resultT[i] = (T)resultObj[i];
            }
            return resultT;
        }


        private static ConcurrentDictionary<Type, string> cache_GetTypeHash = new ConcurrentDictionary<Type, string>();
        public static string GetTypeHash(this Type type)
        {
            return cache_GetTypeHash.GetOrAdd(type, t => {
                using (var sha1 = SHA1.Create())
                {
                    var typeName = t.FullName + ", " + t.Assembly.GetName().Name;
                    var hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(typeName));

                    return Convert.ToBase64String(hashBytes, 0, 12);
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
                .CompileFast(flags: CompilerFlags.ThrowOnNotSupportedExpression))
            .Invoke(d, args);

        public static Type GetResultType(this MemberInfo member) =>
            member is PropertyInfo property ? property.PropertyType :
            member is FieldInfo field ? field.FieldType :
            member is MethodInfo method ? method.ReturnType :
            member is TypeInfo type ? type.AsType() :
            throw new NotImplementedException($"Could not get return type of member {member.GetType().FullName}");

        [return: NotNullIfNotNull("instance")]
        public static string? ToEnumString<T>(T? instance) where T : struct, Enum
        {
            if (instance == null)
                return null;

            if (!EnumInfo<T>.HasEnumMemberField)
            {
                return instance.ToString()!;
            }
            else if (EnumInfo<T>.IsFlags)
            {
                return JsonConvert.DeserializeObject<string>(JsonConvert.ToString(instance.Value));
            }
            else
            {
                var name = instance.ToString()!;
                return ToEnumString(typeof(T), name);
            }
        }

        public static string ToEnumString(Type enumType, string name)
        {
            var field = enumType.GetField(name);
            if (field != null)
            {
                var attr = (EnumMemberAttribute?)field.GetCustomAttributes(typeof(EnumMemberAttribute), false).SingleOrDefault();
                if (attr is { Value: {} })
                {
                    return attr.Value;
                }
            }
            return name;
        }

        internal static class EnumInfo<T> where T: struct, Enum
        {
            internal static readonly bool HasEnumMemberField;
            internal static readonly bool IsFlags;

            static EnumInfo()
            {
                foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (field.IsDefined(typeof(EnumMemberAttribute), false))
                    {
                        HasEnumMemberField = true;
                        break;
                    }
                }
                IsFlags = typeof(T).IsDefined(typeof(FlagsAttribute));
            }
        }
        
        public static Type GetDelegateType(MethodInfo methodInfo)
        {
            return Expression.GetDelegateType(methodInfo.GetParameters().Select(a => a.ParameterType).Append(methodInfo.ReturnType).ToArray());
        }

        /// <summary> Clear cache when hot reload happens </summary>
        internal static void ClearCaches(Type[] types)
        {
            foreach (var t in types)
            {
                delegateInvokeCache.TryRemove(t, out _);
                cache_GetTypeHash.TryRemove(t, out _);
            }
        }

        internal static bool IsInitOnly(this PropertyInfo prop) =>
            prop.SetMethod is { ReturnParameter: {} returnParameter } &&
            returnParameter.GetRequiredCustomModifiers().Any(t => t == typeof(System.Runtime.CompilerServices.IsExternalInit));

        public static IEnumerable<Type> GetBaseTypesAndInterfaces(Type type)
        {
            foreach (var i in type.GetInterfaces())
            {
                yield return i;
            }

            while (type.BaseType is { } baseType)
            {
                yield return baseType;
                type = baseType;
            }
        }
    }
}
