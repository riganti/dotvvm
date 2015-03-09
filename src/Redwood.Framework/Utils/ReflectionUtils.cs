using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Redwood.Framework.Utils
{
    public class ReflectionUtils
    {

        /// <summary>
        /// Gets the property name from lambda expression, e.g. 'a => a.FirstName'
        /// </summary>
        public static string GetPropertyNameFromExpression<T>(Expression<Func<T, object>> expression)
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
                    return Enum.Parse(type, (string)value);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("The enum {0} does not allow a value '{1}'!", type, value), ex);      // TODO: exception handling
                }
            }

            // generic to string
            if (type == typeof (string))
            {
                return value.ToString();
            }

            // convert
            return Convert.ChangeType(value, type);
        }

    }
}
