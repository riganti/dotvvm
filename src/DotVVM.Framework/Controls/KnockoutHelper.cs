using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    public static class KnockoutHelper
    {
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, DotvvmControl control, DotvvmProperty property, Action nullBindingAction = null,
            string valueUpdate = null, bool renderEvenInServerRenderingMode = false, bool setValueBack = false)
        {
            var expression = control.GetValueBinding(property);
            if (expression != null && (!control.RenderOnServer || renderEvenInServerRenderingMode))
            {
                writer.AddAttribute("data-bind", name + ": " + expression.GetKnockoutBindingExpression(), true, ", ");
                if (valueUpdate != null)
                {
                    writer.AddAttribute("data-bind", "valueUpdate: '" + valueUpdate + "'", true, ", ");
                }
            }
            else
            {
                if (nullBindingAction != null) nullBindingAction();
                if (setValueBack && expression != null) control.SetValue(property, expression.Evaluate(control, property));
            }
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IValueBinding valueBinding)
        {
            writer.AddKnockoutDataBind(name, valueBinding.GetKnockoutBindingExpression());
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IEnumerable<KeyValuePair<string, IValueBinding>> expressions, DotvvmControl control, DotvvmProperty property)
        {
            writer.AddAttribute("data-bind", name + ": {" + String.Join(",", expressions.Select(e => "'" + e.Key + "': " + e.Value.GetKnockoutBindingExpression())) + "}", true, ", ");
        }

        public static void WriteKnockoutForeachComment(this IHtmlWriter writer, string binding)
        {
            writer.WriteKnockoutDataBindComment("foreach", binding);
        }

        public static void WriteKnockoutWithComment(this IHtmlWriter writer, string binding)
        {
            writer.WriteKnockoutDataBindComment("with", binding);
        }

        public static void WriteKnockoutDataBindComment(this IHtmlWriter writer, string name, string expression)
        {
            writer.WriteUnencodedText($"<!-- ko { name }: { expression } -->");
        }

        public static void WriteKnockoutDataBindComment(this IHtmlWriter writer, string name, DotvvmControl control, DotvvmProperty property)
        {
            writer.WriteUnencodedText($"<!-- ko { name }: { control.GetValueBinding(property).GetKnockoutBindingExpression() } -->");
        }

        public static void WriteKnockoutDataBindEndComment(this IHtmlWriter writer)
        {
            writer.WriteUnencodedText("<!-- /ko -->");
        }

        public static void AddKnockoutForeachDataBind(this IHtmlWriter writer, string expression)
        {
            writer.AddKnockoutDataBind("foreach", expression);
        }

        public static string GenerateClientPostBackScript(string propertyName, ICommandBinding expression, DotvvmControl control, bool useWindowSetTimeout = false,
            bool? returnValue = false, bool isOnChange = false)
        {
            var uniqueControlId = "";
            if (expression is ControlCommandBindingExpression)
            {
                var target = (DotvvmControl)control.GetClosestControlBindingTarget();
                target.EnsureControlHasId();
                uniqueControlId = target.GetClientId();
            }

            var arguments = new List<string>()
            {
                "'root'",
                "this",
                "[" + String.Join(", ", GetContextPath(control).Reverse().Select(p => '"' + p + '"')) + "]",
                "'" + uniqueControlId + "'",
                useWindowSetTimeout ? "true" : "false",
                JsonConvert.SerializeObject(GetValidationTargetExpression(control)),
                "null",
                GetPostBackHandlersScript(control, propertyName)
            };

            // return the script
            var condition = isOnChange ? "if (!dotvvm.isViewModelUpdating) " : null;
            var returnStatement = returnValue != null ? string.Format(";return {0};", returnValue.ToString().ToLower()) : "";

            // call the function returned from binding js with runtime arguments
            var postBackCall = String.Format("{0}({1})", expression.GetCommandJavascript(), String.Join(", ", arguments));
            return condition + postBackCall + returnStatement;
        }

        /// <summary>
        /// Generates a list of postback update handlers.
        /// </summary>
        private static string GetPostBackHandlersScript(DotvvmControl control, string eventName)
        {
            var handlers = (List<PostBackHandler>)control.GetValue(PostBack.HandlersProperty);
            if (handlers == null) return "null";

            var effectiveHandlers = handlers.Where(h => string.IsNullOrEmpty(h.EventName) || h.EventName == eventName);
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var handler in effectiveHandlers)
            {
                if (sb.Length > 1)
                {
                    sb.Append(",");
                }
                sb.Append("{name:'");
                sb.Append(handler.ClientHandlerName);
                sb.Append("',options:function(){return {");
                foreach (var option in handler.GetHandlerOptionClientExpressions())
                {
                    sb.Append(option.Key);
                    sb.Append(":");
                    sb.Append(option.Value);
                }
                sb.Append("};}}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static IEnumerable<string> GetContextPath(DotvvmControl control)
        {
            while (control != null)
            {
                var pathFragment = control.GetValue(Internal.PathFragmentProperty, false) as string;
                if (pathFragment != null)
                {
                    yield return pathFragment;
                }
                else
                {
                    var dataContextBinding = control.GetBinding(DotvvmBindableObject.DataContextProperty, false) as IValueBinding;
                    if (dataContextBinding != null)
                    {
                        yield return dataContextBinding.GetKnockoutBindingExpression();
                    }
                }
                control = control.Parent;
            }
        }

        /// <summary>
        /// Gets the validation target expression.
        /// </summary>
        public static string GetValidationTargetExpression(DotvvmBindableObject control)
        {
            if (!(bool)control.GetValue(Validation.EnabledProperty))
            {
                return null;
            }

            // find the closest control
            int dataSourceChanges;
            var validationTargetControl = control.GetClosestWithPropertyValue(out dataSourceChanges, c => c.GetValueBinding(Validation.TargetProperty) != null);
            if (validationTargetControl == null)
            {
                return "$root";
            }

            // reparent the expression to work in current DataContext
            // FIXME: This does not work:
            var validationBindingExpression = validationTargetControl.GetValueBinding(Validation.TargetProperty);
            string validationExpression = validationBindingExpression.GetKnockoutBindingExpression();
            validationExpression = string.Join("", Enumerable.Range(0, dataSourceChanges).Select(i => "$parent.")) + validationExpression;

            return validationExpression;
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