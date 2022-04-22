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

        public static string? MergeValues(GroupedDotvvmProperty property, string? a, string? b)
        {
            // for perf reasons only do this compile time - we'll deduplicate the attribute if it's a CSS class
            if (property.GroupMemberName == "class" && a is string && b is string)
            {
                var classesA = a.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                var classesB = b.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                    .Where(c => !classesA.Contains(c));
                b = string.Join(" ", classesB);
            }

            return HtmlWriter.JoinAttributeValues(property.GroupMemberName, a, b);
        }


        public override object? MergePlainValues(DotvvmProperty prop, object? a, object? b)
        {
            var gProp = (GroupedDotvvmProperty)prop;
            if (a is null) return b;
            if (b is null) return a;

            if (a is string aString && b is string bString)
                return HtmlWriter.JoinAttributeValues(gProp.GroupMemberName, aString, bString);

            // append to list. Order does not matter in html attributes
            if (a is HtmlGenericControl.AttributeList alist)
                return new HtmlGenericControl.AttributeList(b, alist);
            else if (b is HtmlGenericControl.AttributeList blist)
                return new HtmlGenericControl.AttributeList(a, blist);

            return new HtmlGenericControl.AttributeList(a, new HtmlGenericControl.AttributeList(b, null));
        }
    }
}
