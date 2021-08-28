using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation
{
    public class HtmlAttributeValueMerger: DefaultAttributeValueMerger
    {
        public static Expression MergeExpressions(GroupedDotvvmProperty property, Expression a, Expression b)
        {
            var attributeName = property.GroupMemberName;
            var separator = HtmlWriter.GetSeparatorForAttribute(attributeName);
            var method = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });
            return Expression.Add(Expression.Add(a, Expression.Constant(separator), method), b, method);
            // return Expression.Call(typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) }), a, Expression.Constant(separator), b);
        }

        public static string? MergeValues(GroupedDotvvmProperty property, string? a, string? b) =>
            HtmlWriter.JoinAttributeValues(property.GroupMemberName, a, b);
    }
}
