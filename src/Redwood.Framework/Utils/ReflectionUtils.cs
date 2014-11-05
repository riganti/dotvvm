using System;
using System.Collections.Generic;
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

    }
}
