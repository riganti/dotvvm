using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;
using Redwood.Framework.Runtime;

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
            writer.AddAttribute("data-bind", name + ": {" + String.Join(",", expressions.Select(e => e.Key + ": " + e.Value.TranslateToClientScript(control, property))) + "}", true, ", ");
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

            var arguments = new List<string>()
            {
                "'" + context.CurrentPageArea + "'",
                "this",
                "[" + String.Join(", ", context.PathFragments.Reverse().Select(f => "'" + f + "'")) + "]",
                "'" + expression.Expression + "'",
                "'" + uniqueControlId + "'"
            };
            if ((bool)control.GetValue(Validate.EnabledProperty))
            {
                var validationTargetExpression = GetValidationTargetExpression(control);
                if (validationTargetExpression != null)
                {
                    arguments.Add("'" + validationTargetExpression + "'");
                }
            }

            // postback without validation
            return String.Format("redwood.postBack({0});return false;", String.Join(", ", arguments));
        }

        /// <summary>
        /// Gets the validation target expression.
        /// </summary>
        public static string GetValidationTargetExpression(RedwoodBindableControl control)
        {
            // find the closest control
            int dataSourceChanges;
            var validationTargetControl = (RedwoodBindableControl)control.GetClosestWithPropertyValue(
                out dataSourceChanges, 
                c => c is RedwoodBindableControl && ((RedwoodBindableControl)c).GetValueBinding(Validate.TargetProperty) != null);
            if (validationTargetControl == null)
            {
                return null;
            }

            // reparent the expression to work in current DataContext
            var valueBindingExpression = validationTargetControl.GetValueBinding(Validate.TargetProperty);
            var validationExpression = valueBindingExpression.TranslateToClientScript(validationTargetControl, Validate.TargetProperty);
            validationExpression = String.Join("", Enumerable.Range(0, dataSourceChanges).Select(i => "$parent.")) + validationExpression;

            return validationExpression;
        }

        /// <summary>
        /// Encodes the string so it can be used in Javascript code.
        /// </summary>
        public static string MakeStringLiteral(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        public static string ConvertToCamelCase(ValidationMessageMode mode)
        {
            var name = mode.ToString();
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }
    }
}
