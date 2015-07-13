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
        
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, RedwoodBindableControl control, RedwoodProperty property, Action nullBindingAction, string valueUpdate = null)
        {
            var expression = control.GetValueBinding(property);
            if (expression != null)
            {
                writer.AddAttribute("data-bind", name + ": " + expression.TranslateToClientScript(control, property), true, ", ");
                if (valueUpdate != null)
                {
                    writer.AddAttribute("data-bind", "valueUpdate: '" + valueUpdate + "'", true, ", ");
                }
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
            writer.AddAttribute("data-bind", name + ": {" + String.Join(",", expressions.Select(e => "'" + e.Key + "': " + e.Value.TranslateToClientScript(control, property))) + "}", true, ", ");
        }

        public static void WriteKnockoutDataBindComment(this IHtmlWriter writer, string name, string expression)
        {
            writer.WriteUnencodedText("<!-- ko " + name + ": " + expression + " -->");
        }

        public static void WriteKnockoutDataBindEndComment(this IHtmlWriter writer)
        {
            writer.WriteUnencodedText("<!-- /ko -->");
        }

        public static void AddKnockoutForeachDataBind(this IHtmlWriter writer, string expression)
        {
            writer.AddKnockoutDataBind("foreach", "redwood.getDataSourceItems(" + expression + ")");
        }

        public static string GenerateClientPostBackScript(CommandBindingExpression expression, RenderContext context, RedwoodBindableControl control, bool useWindowSetTimeout = false, bool? returnValue = false)
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
                "'" + uniqueControlId + "'",
                useWindowSetTimeout ? "true" : "false"
            };
            
            var validationTargetExpression = GetValidationTargetExpression(control, true);
            if (validationTargetExpression != null)
            {
                arguments.Add("'" + validationTargetExpression + "'");
            }

            // return the script
            var returnStatement = returnValue != null ? string.Format("return {0};", returnValue.ToString().ToLower()) : "";
            var postBackCall = String.Format("redwood.postBack({0});", String.Join(", ", arguments));
            return postBackCall + returnStatement;
        }

        /// <summary>
        /// Gets the validation target expression.
        /// </summary>
        public static string GetValidationTargetExpression(RedwoodBindableControl control, bool translateToClientScript)
        {
            if (!(bool)control.GetValue(Validate.EnabledProperty))
            {
                return null;
            }

            // find the closest control
            int dataSourceChanges;
            var validationTargetControl = (RedwoodBindableControl)control.GetClosestWithPropertyValue(
                out dataSourceChanges, 
                c => c is RedwoodBindableControl && ((RedwoodBindableControl)c).GetValueBinding(Validate.TargetProperty, false) != null);
            if (validationTargetControl == null)
            {
                return null;
            }

            // reparent the expression to work in current DataContext
            var validationBindingExpression = validationTargetControl.GetValueBinding(Validate.TargetProperty);
            string validationExpression;
            if (translateToClientScript)
            {
                validationExpression = validationBindingExpression.TranslateToClientScript(control, Validate.TargetProperty);

                var prefix = String.Join("", Enumerable.Range(0, dataSourceChanges).Select(i => "$parentContext."));
                if (!validationExpression.StartsWith("$"))
                {
                    prefix += "$data.";
                }
                return prefix + validationExpression;
            }
            else
            {
                validationExpression = validationBindingExpression.Expression;

                var prefix = String.Join("", Enumerable.Range(0, dataSourceChanges).Select(i => "_parent."));
                return prefix + validationExpression;
            }
        }

        /// <summary>
        /// Encodes the string so it can be used in Javascript code.
        /// </summary>
        public static string MakeStringLiteral(string value)
        {
            return "'" + value.Replace("'", "''") + "'";
        }

        public static string ConvertToCamelCase(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }
    }
}
