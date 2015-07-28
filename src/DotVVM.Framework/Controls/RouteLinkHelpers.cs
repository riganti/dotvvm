using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    public static class RouteLinkHelpers
    {

        private const string RouteParameterPrefix = "Param-";


        public static void WriteRouteLinkHrefAttribute(string routeName, HtmlGenericControl control, IHtmlWriter writer, RenderContext context)
        {
            if (!control.RenderOnServer)
            {
                writer.AddKnockoutDataBind("attr", "{ href: " + GenerateKnockoutHrefExpression(routeName, control, context) + "}");
            }
            else
            {
                writer.AddAttribute("href", EvaluateRouteUrl(routeName, control, context));
            }
        }

        public static string EvaluateRouteUrl(string routeName, HtmlGenericControl control, RenderContext context)
        {
            var coreUrl = GenerateRouteUrlCore(routeName, control, context);

            if ((bool)control.GetValue(Internal.IsSpaPageProperty))
            {
                return "#!/" + coreUrl;
            }
            else
            {
                return context.RequestContext.TranslateVirtualPath(coreUrl);
            }
        }

        private static string GenerateRouteUrlCore(string routeName, HtmlGenericControl control, RenderContext context)
        {
            var route = GetRoute(context, routeName);
            var parameters = ComposeNewRouteParameters(control, context, route);

            // evaluate bindings on server
            foreach (var param in parameters.Where(p => p.Value is BindingExpression).ToList())
            {
                if (param.Value is BindingExpression)
                {
                    EnsureValidBindingType(param.Value as BindingExpression);
                    parameters[param.Key] = ((ValueBindingExpression)param.Value).Evaluate(control, null);   // TODO: see below
                }
            }

            // generate the URL
            return route.BuildUrl(parameters);
        }

        private static RouteBase GetRoute(RenderContext context, string routeName)
        {
            return context.RequestContext.Configuration.RouteTable[routeName];
        }

        public static string GenerateKnockoutHrefExpression(string routeName, HtmlGenericControl control, RenderContext context)
        {
            var link = GenerateRouteLinkCore(routeName, control, context);

            if ((bool)control.GetValue(Internal.IsSpaPageProperty))
            {
                return string.Format("'#!/' + {0}", link);
            }
            else
            {
                return string.Format("'{0}' + {1}", context.RequestContext.TranslateVirtualPath("~/"), link);
            }
        }

        private static string GenerateRouteLinkCore(string routeName, HtmlGenericControl control, RenderContext context)
        {
            var route = GetRoute(context, routeName);
            var parameters = ComposeNewRouteParameters(control, context, route);

            // generate the function call
            var sb = new StringBuilder();
            sb.Append("dotvvm.buildRouteUrl(");
            sb.Append(JsonConvert.SerializeObject(route.Url));
            sb.Append(", {");
            sb.Append(string.Join(", ", parameters.Select(p => TranslateRouteParameter(control, p))));
            sb.Append("})");
            return sb.ToString();
        }

        private static string TranslateRouteParameter(HtmlGenericControl control, KeyValuePair<string, object> param)
        {
            string expression = "";
            if (param.Value is BindingExpression)
            {
                EnsureValidBindingType(param.Value as BindingExpression);

                var binding = param.Value as ValueBindingExpression;
                expression = binding.TranslateToClientScript(control, null); // TODO: pass a special DotvvmProperty for dynamic parameters on this place. The function might need the value in the future.
            }
            else
            {
                expression = JsonConvert.SerializeObject(param.Value);
            }
            return JsonConvert.SerializeObject(param.Key) + ": " + expression;
        }

        private static void EnsureValidBindingType(BindingExpression binding)
        {
            if (binding?.Javascript == null)
            {
                throw new Exception("Only {value: ...} bindings are supported in <dot:RouteLink Param-xxx='' /> attributes!");
            }
        }

        private static Dictionary<string, object> ComposeNewRouteParameters(HtmlGenericControl control, RenderContext context, RouteBase route)
        {
            var parameters = new Dictionary<string, object>(route.DefaultValues);
            foreach (var param in context.RequestContext.Parameters)
            {
                parameters[param.Key] = param.Value;
            }
            foreach (var attr in GetRouteParameters(control))
            {
                var parameterName = attr.Key.Substring(RouteParameterPrefix.Length);
                parameters[parameterName] = attr.Value;

                // remove the attribute because we don't want to be rendered
                control.Attributes.Remove(attr.Key);
            }
            return parameters;
        }

        private static List<KeyValuePair<string, object>> GetRouteParameters(HtmlGenericControl control)
        {
            return control.Attributes.Where(a => a.Key.StartsWith(RouteParameterPrefix)).ToList();
        }

    }
}