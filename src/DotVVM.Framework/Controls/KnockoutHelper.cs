using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Runtime;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Controls
{
    public static class KnockoutHelper
    {
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, DotvvmBindableObject control, DotvvmProperty property, Action nullBindingAction = null,
            string valueUpdate = null, bool renderEvenInServerRenderingMode = false, bool setValueBack = false)
        {
            var expression = control.GetValueBinding(property);
            if (expression != null && (!control.RenderOnServer || renderEvenInServerRenderingMode))
            {
                writer.AddAttribute("data-bind", name + ": " + expression.GetKnockoutBindingExpression(control), true, ", ");
                if (valueUpdate != null)
                {
                    writer.AddAttribute("data-bind", "valueUpdate: '" + valueUpdate + "'", true, ", ");
                }
            }
            else
            {
                nullBindingAction?.Invoke();
                if (setValueBack && expression != null) control.SetValue(property, expression.Evaluate(control));
            }
        }

        [Obsolete("Use the AddKnockoutDataBind(this IHtmlWriter writer, string name, IValueBinding valueBinding, DotvvmControl control) or AddKnockoutDataBind(this IHtmlWriter writer, string name, string expression) overload")]
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IValueBinding valueBinding)
        {
            writer.AddKnockoutDataBind(name, valueBinding.GetKnockoutBindingExpression());
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IValueBinding valueBinding, DotvvmBindableObject control)
        {
            writer.AddKnockoutDataBind(name, valueBinding.GetKnockoutBindingExpression(control));
        }

        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IEnumerable<KeyValuePair<string, IValueBinding>> expressions, DotvvmBindableObject control, DotvvmProperty property)
        {
            writer.AddAttribute("data-bind", name + ": {" + String.Join(",", expressions.Select(e => "'" + e.Key + "': " + e.Value.GetKnockoutBindingExpression(control))) + "}", true, ", ");
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

        public static void WriteKnockoutDataBindComment(this IHtmlWriter writer, string name, DotvvmBindableObject control, DotvvmProperty property)
        {
            writer.WriteUnencodedText($"<!-- ko { name }: { control.GetValueBinding(property).GetKnockoutBindingExpression(control) } -->");
        }

        public static void WriteKnockoutDataBindEndComment(this IHtmlWriter writer)
        {
            writer.WriteUnencodedText("<!-- /ko -->");
        }

        public static void AddKnockoutForeachDataBind(this IHtmlWriter writer, string expression)
        {
            writer.AddKnockoutDataBind("foreach", expression);
        }

        public static string GenerateClientPostBackScript(string propertyName, ICommandBinding expression, DotvvmBindableObject control, bool useWindowSetTimeout = false,
            bool? returnValue = false, bool isOnChange = false, string elementAccessor = "this")
        {
            return GenerateClientPostBackScript(propertyName, expression, control, new PostbackScriptOptions(useWindowSetTimeout, returnValue, isOnChange, elementAccessor));
        }
        public static string GenerateClientPostBackScript(string propertyName, ICommandBinding expression, DotvvmBindableObject control, PostbackScriptOptions options)
        {
            var target = (DotvvmControl)control.GetClosestControlBindingTarget();
            var uniqueControlId = target?.GetDotvvmUniqueId();

            // return the script
            var returnStatement = options.ReturnValue != null ? $";return {options.ReturnValue.ToString().ToLower()};" : "";

            var call = expression.GetParametrizedCommandJavascript(control).ToString(p =>
                p == CommandBindingExpression.ViewModelNameParameter ? new CodeParameterAssignment("'root'", OperatorPrecedence.Max) :
                p == CommandBindingExpression.SenderElementParameter ? options.ElementAccessor :
                p == CommandBindingExpression.CurrentPathParameter ? new CodeParameterAssignment(
                    "[" + String.Join(", ", GetContextPath(control).Reverse().Select(JavascriptCompilationHelper.CompileConstant)) + "]",
                    OperatorPrecedence.Max) :
                p == CommandBindingExpression.ControlUniqueIdParameter ? new CodeParameterAssignment(
                    (uniqueControlId is IValueBinding ? "{ expr: " + JsonConvert.ToString(((IValueBinding)uniqueControlId).GetKnockoutBindingExpression(control), '\'', StringEscapeHandling.Default) + "}" : "'" + (string)uniqueControlId + "'"), OperatorPrecedence.Max) :
                p == CommandBindingExpression.UseObjectSetTimeoutParameter ? new CodeParameterAssignment(options.UseWindowSetTimeout ? "true" : "false", OperatorPrecedence.Max) :
                p == CommandBindingExpression.ValidationPathParameter ? CodeParameterAssignment.FromExpression(new JsLiteral(GetValidationTargetExpression(control))) :
                p == CommandBindingExpression.OptionalKnockoutContextParameter ? new CodeParameterAssignment("null", OperatorPrecedence.Max) :
                p == CommandBindingExpression.PostbackHandlersParameters ? new CodeParameterAssignment(GetPostBackHandlersScript(control, propertyName), OperatorPrecedence.Max) :
                default(CodeParameterAssignment)
                );
            if (options.IsOnChange) call = "if(!dotvvm.isViewModelUpdating){" + call + "}";
            return call + returnStatement;
        }

        /// <summary>
        /// Generates a list of postback update handlers.
        /// </summary>
        private static string GetPostBackHandlersScript(DotvvmBindableObject control, string eventName)
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

                var isFirst = true;
                var options = handler.GetHandlerOptionClientExpressions();
                options.Add("enabled", handler.TranslateValueOrBinding(PostBackHandler.EnabledProperty));
                foreach (var option in options)
                {
                    if (!isFirst)
                    {
                        sb.Append(',');
                    }
                    isFirst = false;

                    sb.Append(option.Key);
                    sb.Append(":");
                    sb.Append(option.Value);
                }
                sb.Append("};}}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static IEnumerable<string> GetContextPath(DotvvmBindableObject control)
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
                        yield return dataContextBinding.GetKnockoutBindingExpression(control);
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
            var validationTargetControl = control.GetClosestControlValidationTarget(out var _);
            if (validationTargetControl == null)
            {
                return "$root";
            }

            // reparent the expression to work in current DataContext
            return validationTargetControl.GetValueBinding(Validation.TargetProperty).GetKnockoutBindingExpression(control);
        }

        /// <summary>
        /// Add knockout data bind comment dotvvm_withControlProperties with the specified properties
        /// </summary>
        public static void AddCommentAliasBinding(IHtmlWriter writer, IDictionary<string, string> properties)
        {
            writer.WriteKnockoutDataBindComment("dotvvm_introduceAlias", "{" + string.Join(", ", properties.Select(p => JsonConvert.ToString(p.Key) + ":" + properties.Values)) + "}");
        }

        /// <summary>
        /// Writes text iff the property contains hard-coded value OR
        /// writes knockout text binding iff the property contains binding
        /// </summary>
        /// <param name="writer">HTML output writer</param>
        /// <param name="obj">Dotvvm control which contains the <see cref="DotvvmProperty"/> with value to be written</param>
        /// <param name="property">Value of this property will be written</param>
        /// <param name="wrapperTag">Name of wrapper tag, null => knockout binding comment</param>
        public static void WriteTextOrBinding(this IHtmlWriter writer, DotvvmBindableObject obj, DotvvmProperty property, string wrapperTag = null)
        {
            var valueBinding = obj.GetValueBinding(property);
            if (valueBinding != null)
            {
                if (wrapperTag == null)
                {
                    writer.WriteKnockoutDataBindComment("text", valueBinding.GetKnockoutBindingExpression(obj));
                    writer.WriteKnockoutDataBindEndComment();
                }
                else
                {
                    writer.AddKnockoutDataBind("text", valueBinding.GetKnockoutBindingExpression(obj));
                    writer.RenderBeginTag(wrapperTag);
                    writer.RenderEndTag();
                }
            }
            else
            {
                if (wrapperTag != null) writer.RenderBeginTag(wrapperTag);
                writer.WriteText(obj.GetValue(property).ToString());
                if (wrapperTag != null) writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Returns Javascript expression that represents the property value (even if the property contains hardcoded value)
        /// </summary>
        public static string GetKnockoutBindingExpression(this DotvvmBindableObject obj, DotvvmProperty property)
        {
            var binding = obj.GetValueBinding(property);
            if (binding != null) return binding.GetKnockoutBindingExpression(obj);
            return JsonConvert.SerializeObject(obj.GetValue(property));
        }

        /// <summary>
        /// Encodes the string so it can be used in Javascript code.
        /// </summary>
        public static string MakeStringLiteral(string value, bool useApos = true)
        {
            return JsonConvert.ToString(value, useApos ? '\'' : '"', StringEscapeHandling.Default);
        }

        public static string ConvertToCamelCase(string name)
        {
            return name.Substring(0, 1).ToLower() + name.Substring(1);
        }
    }
}
