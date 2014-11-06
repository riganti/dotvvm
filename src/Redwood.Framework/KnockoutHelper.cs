using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework
{
    public static class KnockoutHelper
    {

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, ValueBindingExpression expression)
        {
            writer.AddAttribute("data-bind", name + ": " + expression.TranslateToClientScript(), true, ", ");
        }
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, string expression)
        {
            writer.AddAttribute("data-bind", name + ": " + expression, true, ", ");
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IEnumerable<KeyValuePair<string, ValueBindingExpression>> expressions)
        {
            writer.AddAttribute("data-bind", name + ": {" + string.Join(",", expressions.Select(e => e.Key + ": " + e.Value.TranslateToClientScript())) + "}", true, ", ");
        }

        public static void WriteKnockoutDataBindComment(this IHtmlWriter writer, string name, ValueBindingExpression expression)
        {
            writer.WriteUnencodedText("<!-- ko " + name + ": " + expression.TranslateToClientScript() + " -->");
        }

        public static void WriteKnockoutDataBindEndComment(this IHtmlWriter writer)
        {
            writer.WriteUnencodedText("<!-- /ko -->");
        }

        public static string GenerateClientPostBackScript(CommandBindingExpression expression, RenderContext context)
        {
            return string.Format("redwood.postBack('{0}', this, [{1}], '{2}');return false;",
                context.CurrentPageArea, 
                string.Join(", ", context.PathFragments.Reverse().Select(f => "'" + f + "'")),
                expression.Expression
            );
        }

        /// <summary>
        /// Encodes the string so it can be used in Javascript code.
        /// </summary>
        public static string MakeStringLiteral(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }
    }
}
