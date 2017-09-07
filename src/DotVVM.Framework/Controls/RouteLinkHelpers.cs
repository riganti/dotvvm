using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Controls
{
    public static class RouteLinkHelpers
    {

        private const string RouteParameterPrefix = "Param-";
        private const string RouteQueryPrefix = "Query-";


        public static void WriteRouteLinkHrefAttribute(string routeName, HtmlGenericControl control, DotvvmProperty urlSuffixProperty, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!control.RenderOnServer)
            {
                var group = new KnockoutBindingGroup();
                group.Add("href", GenerateKnockoutHrefExpression(routeName, control, urlSuffixProperty, context));
                writer.AddKnockoutDataBind("attr", group);
            }
            else
            {
                writer.AddAttribute("href", EvaluateRouteUrl(routeName, control, urlSuffixProperty, context));
            }
        }

        public static string EvaluateRouteUrl(string routeName, HtmlGenericControl control, DotvvmProperty urlSuffixProperty, IDotvvmRequestContext context)
        {
            var urlSuffix = GenerateUrlSuffixCore(control.GetValue(urlSuffixProperty) as string, control);
            var coreUrl = GenerateRouteUrlCore(routeName, control, context) + urlSuffix;

            if ((bool)control.GetValue(Internal.IsSpaPageProperty))
            {
                return "#!/" + coreUrl;
            }
            else
            {
                return context.TranslateVirtualPath(coreUrl);
            }
        }

        private static string GenerateRouteUrlCore(string routeName, HtmlGenericControl control, IDotvvmRequestContext context)
        {
            var route = GetRoute(context, routeName);
            var parameters = ComposeNewRouteParameters(control, context, route);

            // evaluate bindings on server
            foreach (var param in parameters.Where(p => p.Value is IStaticValueBinding).ToList())
            {
                EnsureValidBindingType(param.Value as BindingExpression);
                parameters[param.Key] = ((ValueBindingExpression)param.Value).Evaluate(control);   // TODO: see below
            }

            // generate the URL
            return route.BuildUrl(parameters);
        }

        private static string GenerateUrlSuffixCore(string urlSuffix, HtmlGenericControl control)
        {
            var parameters = ComposeNewQueryParameters(control);

            // evaluate bindings on server
            foreach (var param in parameters.Where(p => p.Value is IStaticValueBinding).ToList())
            {
                EnsureValidBindingType(param.Value as BindingExpression);
                parameters[param.Key] = ((ValueBindingExpression)param.Value).Evaluate(control);
            }

            // generate the URL suffix
            return UrlHelper.BuildUrlSuffix(urlSuffix, parameters);
        }

        private static RouteBase GetRoute(IDotvvmRequestContext context, string routeName)
        {
            return context.Configuration.RouteTable[routeName];
        }

        public static string GenerateKnockoutHrefExpression(string routeName, HtmlGenericControl control, DotvvmProperty urlSuffixProperty, IDotvvmRequestContext context)
        {
            var link = GenerateRouteLinkCore(routeName, control, context);

            var urlSuffix = GetUrlSuffixExpression(control, urlSuffixProperty);
            if ((bool)control.GetValue(Internal.IsSpaPageProperty))
            {
                return $"'#!/' + {link} + {urlSuffix}";
            }
            else
            {
                return $"'{context.TranslateVirtualPath("~/")}' + {link} + {urlSuffix}";
            }
        }

        private static string GetUrlSuffixExpression(HtmlGenericControl control, DotvvmProperty urlSuffixProperty)
        {
            var query = ComposeNewQueryParameters(control);
            string urlSuffixBase;

            var urlSuffixBinding = control.GetValueBinding(urlSuffixProperty);
            if (urlSuffixBinding != null)
            {
                urlSuffixBase =  "(" + urlSuffixBinding.GetKnockoutBindingExpression(control) + ")";
            }
            else
            {
                urlSuffixBase = JsonConvert.SerializeObject(control.GetValue(urlSuffixProperty) as string ?? "");
            } 
            // generate the function call
            var sb = new StringBuilder();
            sb.Append("dotvvm.buildUrlSuffix(");
            sb.Append(urlSuffixBase);
            sb.Append(", {");
            sb.Append(string.Join(", ", query.Select(p => TranslateRouteParameter(control, p, true))));
            sb.Append("})");
            return sb.ToString();
        }

        private static string GenerateRouteLinkCore(string routeName, HtmlGenericControl control, IDotvvmRequestContext context)
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

        private static string TranslateRouteParameter(HtmlGenericControl control, KeyValuePair<string, object> param, bool caseSensitive = false)
        {
            string expression = "";
            if (param.Value is IBinding)
            {
                EnsureValidBindingType(param.Value as IBinding);

                expression = (param.Value as IValueBinding)?.GetKnockoutBindingExpression(control)
                    ?? JsonConvert.SerializeObject((param.Value as IStaticValueBinding)?.Evaluate(control), DefaultViewModelSerializer.CreateDefaultSettings());
            }
            else
            {
                expression = JsonConvert.SerializeObject(param.Value, DefaultViewModelSerializer.CreateDefaultSettings());
            }
            return JsonConvert.SerializeObject(caseSensitive ? param.Key : param.Key.ToLower()) + ": " + expression;
        }

        private static void EnsureValidBindingType(IBinding binding)
        {
            if (!(binding is IStaticValueBinding))
            {
                throw new Exception("Only value bindings are supported in <dot:RouteLink Param-xxx='' /> attributes!");
            }
        }

        private static Dictionary<string, object> ComposeNewQueryParameters(HtmlGenericControl control)
        {
            var query = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var attr in GetRouteQueryParameters(control))
            {
                var parameterName = attr.Key.Substring(RouteQueryPrefix.Length);
                query[parameterName] = attr.Value;

                // remove the attribute because we don't want to be rendered
                control.Attributes.Remove(attr.Key);
            }
            return query;
        }

        private static List<KeyValuePair<string, object>> GetRouteQueryParameters(HtmlGenericControl control)
        {
            return control.Attributes.Where(a => a.Key.StartsWith(RouteQueryPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private static Dictionary<string, object> ComposeNewRouteParameters(HtmlGenericControl control, IDotvvmRequestContext context, RouteBase route)
        {
            var parameters = new Dictionary<string, object>(route.DefaultValues, StringComparer.OrdinalIgnoreCase);
            foreach (var param in context.Parameters)
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
            return control.Attributes.Where(a => a.Key.StartsWith(RouteParameterPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
