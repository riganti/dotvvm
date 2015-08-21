using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using Newtonsoft.Json;

namespace DotVVM.Framework
{
    public static class KnockoutHelper
    {
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, DotvvmBindableControl control, DotvvmProperty property, Action nullBindingAction = null, string valueUpdate = null)
        {
            var expression = control.GetBinding(property);
            if (expression is IValueBinding)
            {
                writer.AddAttribute("data-bind", name + ": " + (expression as IValueBinding).GetKnockoutBindingExpression(), true, ", ");
                if (valueUpdate != null)
                {
                    writer.AddAttribute("data-bind", "valueUpdate: '" + valueUpdate + "'", true, ", ");
                }
            }
            else
            {
                if (nullBindingAction != null) nullBindingAction();
            }
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IEnumerable<KeyValuePair<string, IValueBinding>> expressions, DotvvmBindableControl control, DotvvmProperty property)
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

        public static void WriteKnockoutDataBindEndComment(this IHtmlWriter writer)
        {
            writer.WriteUnencodedText("<!-- /ko -->");
        }

        public static void AddKnockoutForeachDataBind(this IHtmlWriter writer, string expression)
        {
            writer.AddKnockoutDataBind("foreach", expression);
        }

        public static string GenerateClientPostBackScript(BindingExpression expression, RenderContext context, DotvvmBindableControl control, bool useWindowSetTimeout = false, bool? returnValue = false, bool isOnChange = false)
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
                "[" + String.Join(", ", GetContextPath(control).Reverse().Select(p => '"' + p + '"')) + "]",
                "'" + uniqueControlId + "'",
                useWindowSetTimeout ? "true" : "false",
                JsonConvert.SerializeObject(GetValidationTargetExpression(control))
            };

            // return the script
            var condition = isOnChange ? "if (!dotvvm.isViewModelUpdating) " : "";
            var returnStatement = returnValue != null ? string.Format("return {0};", returnValue.ToString().ToLower()) : "";
            // call the function returned from binding js with runtime arguments
            var postBackCall = String.Format("({0})({1});", expression.Javascript, String.Join(", ", arguments));
            return condition + postBackCall + returnStatement;
        }

        private static IEnumerable<string> GetContextPath(DotvvmControl control)
        {
            while (control != null)
            {
                if (control is DotvvmBindableControl)
                {
                    var pathFragment = ((DotvvmBindableControl)control).GetValue(Internal.PathFragmentProperty, false) as string;
                    if (pathFragment != null)
                    {
                        yield return pathFragment;
                    }
                    else
                    {
                        var dataContextBinding = ((DotvvmBindableControl)control).GetBinding(DotvvmBindableControl.DataContextProperty, false) as IValueBinding;
                        if (dataContextBinding != null)
                        {
                            yield return dataContextBinding.GetKnockoutBindingExpression();
                        }
                    }
                }
                control = control.Parent;
            }
        }

        /// <summary>
        /// Gets the validation target expression.
        /// </summary>
        public static string GetValidationTargetExpression(DotvvmBindableControl control)
        {
            if (!(bool)control.GetValue(Validate.EnabledProperty))
            {
                return null;
            }

            // find the closest control
            int dataSourceChanges;
            var validationTargetControl = (DotvvmBindableControl)control.GetClosestWithPropertyValue(
                out dataSourceChanges,
                c => c is DotvvmBindableControl && ((DotvvmBindableControl)c).GetValueBinding(Validate.TargetProperty) != null);
            if (validationTargetControl == null)
            {
                return "$root";
            }

            // reparent the expression to work in current DataContext
            var validationBindingExpression = validationTargetControl.GetValueBinding(Validate.TargetProperty);
            string validationExpression = validationBindingExpression.GetKnockoutBindingExpression();
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

        public static string ConvertToCamelCase(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }
    }
}
