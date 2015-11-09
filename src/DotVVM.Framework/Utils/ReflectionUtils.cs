using DotVVM.Framework.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace DotVVM.Framework.Utils
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Gets the property name from lambda expression, e.g. 'a => a.FirstName'
        /// </summary>
        public static string GetPropertyNameFromExpression<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                var unaryExpressionBody = (UnaryExpression)expression.Body;
                body = unaryExpressionBody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }

        /// <summary>
        /// Gets the specified property of a given object.
        /// </summary>
        public static object GetObjectProperty(object item, string propertyName)
        {
            if (item == null) return null;

            var type = item.GetType();
            var prop = type.GetProperty(propertyName);
            if (prop == null)
            {
                throw new Exception(String.Format("The object of type {0} does not have a property named {1}!", type, propertyName));     // TODO: exception handling
            }
            return prop.GetValue(item);
        }

        /// <summary>
        /// Extracts the value of a specified property and converts it to string. If the property name is empty, returns a string representation of a given object.
        /// Null values are converted to empty string.
        /// </summary>
        public static string ExtractMemberStringValue(object item, string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                item = GetObjectProperty(item, propertyName);
            }
            return item != null ? item.ToString() : "";
        }

        /// <summary>
        /// Converts a value to a specified type
        /// </summary>
        public static object ConvertValue(object value, Type type)
        {
            // handle null values
            if ((value == null) && (type.IsValueType))
                return Activator.CreateInstance(type);

            // handle nullable types
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if ((value is string) && ((string)value == string.Empty))
                {
                    // value is an empty string, return null
                    return null;
                }
                else
                {
                    // value is not null
                    var nullableConverter = new NullableConverter(type);
                    type = nullableConverter.UnderlyingType;
                }
            }

            // handle exceptions
            if ((value is string) && (type == typeof(Guid)))
                return new Guid((string)value);
            if (type == typeof(object)) return value;

            // handle enums
            if (type.IsEnum && value is string)
            {
                try
                {
                    return Enum.Parse(type, (string)value, true);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("The enum {0} does not allow a value '{1}'!", type, value), ex);      // TODO: exception handling
                }
            }

            // generic to string
            if (type == typeof(string))
            {
                return value.ToString();
            }

            if (value is string && type.IsArray)
            {
                var str = value as string;
                if (type == typeof(string[]))
                    return str.Split(',');
            }

            // convert
            return Convert.ChangeType(value, type);
        }

        public static Type FindType(string name, bool ignoreCase = false)
        {
            var stringComparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            // Type.GetType might sometimes work well
            var type = Type.GetType(name, false, ignoreCase);
            if (type != null) return type;

            var split = name.Split(',');
            name = split[0];

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (split.Length > 1)
            {
                var assembly = split[1];
                type = assemblies.Where(a => a.GetName().Name == assembly).Select(a => a.GetType(name)).FirstOrDefault(t => t != null);
                if (type != null) return type;
            }

            type = assemblies.Where(a => name.StartsWith(a.GetName().Name, stringComparison)).Select(a => a.GetType(name, false, ignoreCase)).FirstOrDefault(t => t != null);
            if (type != null) return type;
            return assemblies.Select(a => a.GetType(name, false, ignoreCase)).FirstOrDefault(t => t != null);
        }

        public static Type GetEnumerableType(Type collectionType)
        {
            // handle array
            if (collectionType.IsArray)
            {
                return collectionType.GetElementType();
            }

            // handle iEnumerables
            Type iEnumerable;
            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                iEnumerable = collectionType;
            }
            else
            {
                iEnumerable = collectionType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }
            if (iEnumerable != null)
            {
                return iEnumerable.GetGenericArguments()[0];
            }

            // handle GridViewDataSet
            if (typeof(IGridViewDataSet).IsAssignableFrom(collectionType))
            {
                var itemsType = collectionType.GetProperty(nameof(IGridViewDataSet.Items)).PropertyType;
                return GetEnumerableType(itemsType);
            }

            // handle object collections
            if (typeof(IEnumerable).IsAssignableFrom(collectionType))
            {
                return typeof(object);
            }

            throw new NotSupportedException();      // TODO
        }

        private static readonly List<Type> NumericTypes = new List<Type>()
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

        public static bool IsNumericType(this Type type)
        {
            return NumericTypes.Contains(type);
        }

        public static bool IsDynamicOrObject(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider)) ||
                   type == typeof(object);
        }

        public static bool IsDelegate(this Type type)
        {
            return typeof(Delegate).IsAssignableFrom(type.BaseType);
        }

        public static bool IsReferenceType(this Type type)
        {
            return type.IsArray || type.IsClass || type.IsInterface || type.IsDelegate();
        }

        public static bool IsDerivedFrom(this Type T, Type superClass)
        {
            return superClass.IsAssignableFrom(T);
        }

        public static bool Implements(this Type T, Type interfaceType)
        {
            return T.GetInterfaces().Any(x =>
            {
                return x.Name == interfaceType.Name;
            });
        }

        public static bool IsDynamic(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(IDynamicMetaObjectProvider));
        }

        public static bool IsObject(this Type type)
        {
            return type == typeof(Object);
        }

        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}