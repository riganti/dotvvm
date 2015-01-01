using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework
{
    public static class KnockoutHelper
    {
        
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, RedwoodBindableControl control, RedwoodProperty property, Action nullBindingAction)
        {
            var expression = control.GetValueBinding(property);
            if (expression != null)
            {
                writer.AddAttribute("data-bind", name + ": " + expression.TranslateToClientScript(control, property), true, ", ");
            }
            else
            {
                nullBindingAction();
            }
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, string expression)
        {
            writer.AddAttribute("data-bind", name + ": " + expression, true, ", ");
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IEnumerable<KeyValuePair<string, ValueBindingExpression>> expressions, RedwoodBindableControl control, RedwoodProperty property)
        {
            writer.AddAttribute("data-bind", name + ": {" + string.Join(",", expressions.Select(e => e.Key + ": " + e.Value.TranslateToClientScript(control, property))) + "}", true, ", ");
        }

        public static string GenerateClientPostBackScript(CommandBindingExpression expression, RenderContext context, RedwoodBindableControl control)
        {
            var uniqueControlId = "";
            if (expression is ControlCommandBindingExpression)
            {
                var target = control.GetClosestControlBindingTarget();
                target.EnsureControlHasId();
                uniqueControlId = target.ID;
            }

            return string.Format("redwood.postBack('{0}', this, [{1}], '{2}', '{3}');return false;",
                context.CurrentPageArea, 
                string.Join(", ", context.PathFragments.Reverse().Select(f => "'" + f + "'")),
                expression.Expression,
                uniqueControlId
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
