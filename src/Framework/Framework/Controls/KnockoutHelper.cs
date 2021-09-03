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
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Configuration;

namespace DotVVM.Framework.Controls
{
    public static class KnockoutHelper
    {
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, DotvvmBindableObject control, DotvvmProperty property, Action? nullBindingAction = null,
            string? valueUpdate = null, bool renderEvenInServerRenderingMode = false, bool setValueBack = false)
        {
            var expression = control.GetValueBinding(property);
            if (expression != null && (!control.RenderOnServer || renderEvenInServerRenderingMode))
            {
                writer.AddKnockoutDataBind(name, expression.GetKnockoutBindingExpression(control));
                if (valueUpdate != null)
                {
                    writer.AddKnockoutDataBind("valueUpdate", $"'{valueUpdate}'");
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

        /// <param name="property">This parameter is here for historical reasons, it's not useful for anything</param>
        public static void AddKnockoutDataBind(this IHtmlWriter writer, string name, IEnumerable<KeyValuePair<string, IValueBinding>> expressions, DotvvmBindableObject control, DotvvmProperty? property = null)
        {
            writer.AddKnockoutDataBind(name, $"{{{String.Join(",", expressions.Select(e => "'" + e.Key + "': " + e.Value.GetKnockoutBindingExpression(control)))}}}");
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
            if (name.Contains("-->") || expression.Contains("-->"))
                throw new Exception("Knockout data bind comment can't contain substring '-->'. If you have discovered this exception in your log, you probably have a XSS vulnerability in you website.");

            writer.WriteUnencodedText($"<!-- ko { name }: { expression } -->");
        }

        public static void WriteKnockoutDataBindComment(this IHtmlWriter writer, string name, DotvvmBindableObject control, DotvvmProperty property)
        {
            var binding = control.GetValueBinding(property);
            if (binding is null) throw new DotvvmControlException(control, $"A value binding expression was expected in property {property}.");
            writer.WriteKnockoutDataBindComment(name, binding.GetKnockoutBindingExpression(control));
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
            var expr = GenerateClientPostBackExpression(propertyName, expression, control, options);
            expr += ".catch(dotvvm.log.logPostBackScriptError)";
            if (options.ReturnValue == false)
                return expr + ";event.stopPropagation();return false;";
            else
                return expr;
        }

        public static string GenerateClientPostBackExpression(string propertyName, ICommandBinding expression, DotvvmBindableObject control, PostbackScriptOptions options)
        {
            var target = (DotvvmControl?)control.GetClosestControlBindingTarget();
            var uniqueControlId = target?.GetDotvvmUniqueId();

            string getContextPath(DotvvmBindableObject? current)
            {
                var result = new List<string>();
                while (current != null)
                {
                    var pathFragment = current.GetDataContextPathFragment();
                    if (pathFragment != null)
                    {
                        result.Add(JsonConvert.ToString(pathFragment));
                    }
                    current = current.Parent;
                }
                result.Reverse();
                return "[" + string.Join(",", result) + "]";
            }

            string getHandlerScript()
            {
                if (!options.AllowPostbackHandlers) return "[]";
                // turn validation off for static commands
                var validationPath = expression is IStaticCommandBinding ? null : GetValidationTargetExpression(control);
                return GetPostBackHandlersScript(control, propertyName,
                    // validation handler
                    validationPath == null ? null :
                    validationPath == RootValidationTargetExpression ? "\"validate-root\"" :
                    validationPath == "$data" ? "\"validate-this\"" :
                    $"[\"validate\", {{path:{JsonConvert.ToString(validationPath)}}}]",

                    // use window.setTimeout
                    options.UseWindowSetTimeout ? "\"timeout\"" : null,

                    options.IsOnChange ? "\"suppressOnUpdating\"" : null,

                    GenerateConcurrencyModeHandler(propertyName, control)
                );
            }
            var (isStaticCommand, jsExpression) =
                expression switch {
                    IStaticCommandBinding { OptionsLambdaJavascript: var optionsLambdaExpression } => (true, optionsLambdaExpression),
                    _ => (false, expression.CommandJavascript)
                };
            var adjustedExpression = 
                JavascriptTranslator.AdjustKnockoutScriptContext(jsExpression,
                    dataContextLevel: BindingHelper.FindDataContextTarget(expression, control).stepsUp);
            // when the expression changes the dataContext, we need to override the default knockout context fo the command binding.
            CodeParameterAssignment knockoutContext;
            CodeParameterAssignment viewModel = default;
            if (!isStaticCommand)
            {
                knockoutContext = options.KoContext ?? (
                    // adjustedExpression != expression.CommandJavascript ?
                    new CodeParameterAssignment(new ParametrizedCode.Builder { "ko.contextFor(", options.ElementAccessor.Code!, ")" }.Build(OperatorPrecedence.Max))
                );
                viewModel = JavascriptTranslator.KnockoutViewModelParameter.DefaultAssignment.Code;
            }
            else
            {
                knockoutContext = CodeParameterAssignment.FromIdentifier("options.knockoutContext");
                viewModel = CodeParameterAssignment.FromIdentifier("options.viewModel");
            }
            var abortSignal = options.AbortSignal ?? CodeParameterAssignment.FromIdentifier("undefined");

            var optionalKnockoutContext =
                options.KoContext is object && adjustedExpression != jsExpression ?
                knockoutContext :
                default;

            var call = SubstituteArguments(adjustedExpression);

            if (isStaticCommand)
            {
                var commandArgsString = (options.CommandArgs?.Code != null) ? SubstituteArguments(options.CommandArgs!.Value.Code!) : "[]";
                var args = new List<string> {
                    SubstituteArguments(options.ElementAccessor.Code!),
                    getHandlerScript(),
                    commandArgsString,
                    optionalKnockoutContext.Code?.Apply(SubstituteArguments) ?? "undefined",
                    SubstituteArguments(abortSignal.Code!)
                };

                // remove default values to reduce mess in generated HTML :)
                if (args.Last() == "undefined")
                {
                    args.RemoveAt(4);
                    if (args.Last() == "undefined")
                    {
                        args.RemoveAt(3);
                        if (args.Last() == "[]")
                        {
                            args.RemoveAt(2);
                            if (args.Last() == "[]")
                                args.RemoveAt(1);
                        }
                    }
                }

                return $"dotvvm.applyPostbackHandlers({call},{string.Join(",", args)})";
            }
            else return call;

            string SubstituteArguments(ParametrizedCode parametrizedCode)
            {
                return parametrizedCode.ToString(p =>
                    p == CommandBindingExpression.SenderElementParameter ? options.ElementAccessor :
                    p == CommandBindingExpression.CurrentPathParameter ? CodeParameterAssignment.FromIdentifier(getContextPath(control)) :
                    p == CommandBindingExpression.ControlUniqueIdParameter ? (
                        uniqueControlId is IValueBinding ?
                            ((IValueBinding)uniqueControlId).GetParametrizedKnockoutExpression(control) :
                            CodeParameterAssignment.FromIdentifier(MakeStringLiteral((string)uniqueControlId!))
                        ) :
                    p == JavascriptTranslator.KnockoutContextParameter ? knockoutContext :
                    p == JavascriptTranslator.KnockoutViewModelParameter ? viewModel :
                    p == CommandBindingExpression.OptionalKnockoutContextParameter ? optionalKnockoutContext :
                    p == CommandBindingExpression.CommandArgumentsParameter ? options.CommandArgs ?? default :
                    p == CommandBindingExpression.PostbackHandlersParameter ? CodeParameterAssignment.FromIdentifier(getHandlerScript()) :
                    p == CommandBindingExpression.AbortSignalParameter ? abortSignal :
                    default
                );
            }
        }

        /// <summary>
        /// Generates a list of postback update handlers.
        /// </summary>
        private static string GetPostBackHandlersScript(DotvvmBindableObject control, string eventName, params string?[] moreHandlers)
        {
            var handlers = (List<PostBackHandler>?)control.GetValue(PostBack.HandlersProperty);
            if ((handlers == null || handlers.Count == 0) && moreHandlers.Length == 0) return "[]";

            var sb = new StringBuilder();
            sb.Append('[');
            if (handlers != null) foreach (var handler in handlers)
                {
                    if (!string.IsNullOrEmpty(handler.EventName) && handler.EventName != eventName) continue;

                    var options = handler.GetHandlerOptions();
                    var name = handler.ClientHandlerName;

                    if (handler.GetValueBinding(PostBackHandler.EnabledProperty) is IValueBinding binding) options.Add("enabled", binding);
                    else if (!handler.Enabled) continue;

                    if (sb.Length > 1)
                        sb.Append(',');

                    if (options.Count == 0)
                    {
                        sb.Append(JsonConvert.ToString(name));
                    }
                    else
                    {
                        string script = GenerateHandlerOptions(handler, options);

                        sb.Append("[");
                        sb.Append(JsonConvert.ToString(name));
                        sb.Append(",");
                        sb.Append(script);
                        sb.Append("]");
                    }
                }
            if (moreHandlers != null) foreach (var h in moreHandlers) if (h != null)
                    {
                        if (sb.Length > 1)
                            sb.Append(',');
                        sb.Append(h);
                    }
            sb.Append(']');
            return sb.ToString();
        }

        private static string GenerateHandlerOptions(DotvvmBindableObject handler, Dictionary<string, object?> options)
        {
            JsExpression optionsExpr = new JsObjectExpression(
                options.Where(o => o.Value != null).Select(o => new JsObjectProperty(o.Key, TransformOptionValueToExpression(handler, o.Value)))
            );

            if (options.Any(o => o.Value is IValueBinding))
            {
                optionsExpr = new JsArrowFunctionExpression(
                    new[] { new JsIdentifier("c"), new JsIdentifier("d") },
                    optionsExpr
                );
            }

            optionsExpr.FixParenthesis();
            var script = new JsFormattingVisitor().ApplyAction(optionsExpr.AcceptVisitor).GetParameterlessResult();
            return script;
        }

        private static JsExpression TransformOptionValueToExpression(DotvvmBindableObject handler, object? optionValue)
        {
            switch (optionValue)
            {
                case IValueBinding binding: {
                    var adjustedCode = binding.GetParametrizedKnockoutExpression(handler, unwrapped: true).AssignParameters(o =>
                        o == JavascriptTranslator.KnockoutContextParameter ? new ParametrizedCode("c") :
                        o == JavascriptTranslator.KnockoutViewModelParameter ? new ParametrizedCode("d") :
                        default(CodeParameterAssignment)
                    );
                    return new JsSymbolicParameter(new CodeSymbolicParameter("tmp symbol", defaultAssignment: adjustedCode));
                }
                case IStaticValueBinding staticValueBinding:
                    return new JsLiteral(staticValueBinding.Evaluate(handler));
                case JsExpression expression:
                    return expression.Clone();
                case IBinding _:
                    throw new ArgumentException("Option value can contains only IValueBinding or IStaticValueBinding. Other bindings are not supported.");
                default:
                    return new JsLiteral(optionValue);
            }
        }

        static string? GenerateConcurrencyModeHandler(string propertyName, DotvvmBindableObject obj)
        {
            var mode = (obj.GetValue(PostBack.ConcurrencyProperty) as PostbackConcurrencyMode?) ?? PostbackConcurrencyMode.Default;

            // determine concurrency queue
            string? queueName = null;
            var queueSettings = obj.GetValueRaw(PostBack.ConcurrencyQueueSettingsProperty) as ConcurrencyQueueSettingsCollection;
            if (queueSettings != null)
            {
                queueName = queueSettings.FirstOrDefault(q => string.Equals(q.EventName, propertyName, StringComparison.OrdinalIgnoreCase))?.ConcurrencyQueue;
            }
            if (queueName == null)
            {
                queueName = obj.GetValue(PostBack.ConcurrencyQueueProperty) as string ?? "default";
            }

            // return the handler script
            if (mode == PostbackConcurrencyMode.Default && "default".Equals(queueName))
            {
                return null;
            }
            var handlerName = $"concurrency-{mode.ToString().ToLowerInvariant()}";
            if ("default".Equals(queueName))
            {
                return JsonConvert.ToString(handlerName);
            }
            else
            {
                return $"[{JsonConvert.ToString(handlerName)},{GenerateHandlerOptions(obj, new Dictionary<string, object?> { ["q"] = queueName })}]";
            }
        }

        private const string RootValidationTargetExpression = "dotvvm.viewModelObservables['root']";

        /// <summary>
        /// Gets the validation target expression.
        /// </summary>
        public static string? GetValidationTargetExpression(DotvvmBindableObject control)
        {
            if (!(bool)control.GetValue(Validation.EnabledProperty)!)
            {
                return null;
            }

            return control.GetValueBinding(Validation.TargetProperty)?.GetKnockoutBindingExpression(control) ??
                   RootValidationTargetExpression;
        }

        /// <summary>
        /// Writes text iff the property contains hard-coded value OR
        /// writes knockout text binding iff the property contains binding
        /// </summary>
        /// <param name="writer">HTML output writer</param>
        /// <param name="obj">Dotvvm control which contains the <see cref="DotvvmProperty"/> with value to be written</param>
        /// <param name="property">Value of this property will be written</param>
        /// <param name="wrapperTag">Name of wrapper tag, null => knockout binding comment</param>
        public static void WriteTextOrBinding(this IHtmlWriter writer, DotvvmBindableObject obj, DotvvmProperty property, string? wrapperTag = null)
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
                writer.WriteText(obj.GetValue(property) + "");
                if (wrapperTag != null) writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Returns Javascript expression that represents the property value (even if the property contains hard-coded value)
        /// </summary>
        public static string GetKnockoutBindingExpression(this DotvvmBindableObject obj, DotvvmProperty property)
        {
            var binding = obj.GetValueBinding(property);
            if (binding != null) return binding.GetKnockoutBindingExpression(obj);
            return JsonConvert.SerializeObject(obj.GetValue(property), DefaultSerializerSettingsProvider.Instance.Settings);
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
            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }
    }
}
