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
            if (expression?.Javascript != null)
            {
                writer.AddAttribute("data-bind", name + ": " + expression.TranslateToClientScript(control, property), true, ", ");
                if (valueUpdate != null)
                {
                    writer.AddAttribute("data-bind", "valueUpdate: '" + valueUpdate + "'", true, ", ");
                }
            }
            else
            {
                if(nullBindingAction != null) nullBindingAction();
            }
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, string expression)
        {
            writer.AddAttribute("data-bind", name + ": " + expression, true, ", ");
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IEnumerable<KeyValuePair<string, ValueBindingExpression>> expressions, DotvvmBindableControl control, DotvvmProperty property)
        {
            writer.AddAttribute("data-bind", name + ": {" + String.Join(",", expressions.Select(e => "'" + e.Key + "': " + e.Value.TranslateToClientScript(control, property))) + "}", true, ", ");
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
            writer.AddKnockoutDataBind("foreach", "dotvvm.getDataSourceItems(" + expression + ")");
            //writer.SetPathFragment(JavascriptCompilationHelper.AddIndexerToViewModel(expression, "$index"));
        }

        public static string GenerateClientPostBackScript(CommandBindingExpression expression, RenderContext context, DotvvmBindableControl control, bool useWindowSetTimeout = false)
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
                "'" + expression.BindingId + "'",
                "'" + uniqueControlId + "'",
                useWindowSetTimeout ? "true" : "false"
            };

            var validationTargetExpression = GetValidationTargetExpression(control);
            if (validationTargetExpression != null)
            {
                arguments.Add("'" + validationTargetExpression + "'");
            }

            // postback without validation
            return String.Format("dotvvm.postBack({0});return false;", String.Join(", ", arguments));
        }
        private static IEnumerable<string> GetContextPath(DotvvmControl control)
        {
            while (control != null)
            {
                if (control is DotvvmBindableControl)
                {
                    var dataContextBinding = ((DotvvmBindableControl)control).GetBinding(DotvvmBindableControl.DataContextProperty, false) as ValueBindingExpression;
                    var pathFragment = ((DotvvmBindableControl)control).GetValue(Internal.PathFragmentProperty, false) as string;
                    if (pathFragment != null)
                    {
                        yield return pathFragment;
                    }
                    if (dataContextBinding != null)
                    {
                        yield return dataContextBinding.Javascript;
                    }
                }
                control = control.Parent;
            }
        }

        public static string GenerateClientPostbackScript(StaticCommandBindingExpression expression, RenderContext context, DotvvmBindableControl control)
        {
            var args = string.Join(", ", expression.GetArgumentPaths().Select(f => "'" + f + "'"));
            var command = expression.GetMethodName(control);
            return $"dotvvm.staticCommandPostback('{ context.CurrentPageArea }', this, '{ command }', [ { args } ]);return false;";
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
                return null;
            }

            // reparent the expression to work in current DataContext
            var validationBindingExpression = validationTargetControl.GetValueBinding(Validate.TargetProperty);
            string validationExpression = validationBindingExpression.TranslateToClientScript(control, Validate.TargetProperty);
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
