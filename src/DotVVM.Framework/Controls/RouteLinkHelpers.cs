#nullable enable
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
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    public static class RouteLinkHelpers
    {
        public static void WriteRouteLinkHrefAttribute(RouteLink control, IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // Render client-side knockout expression only if there exists a parameter with value binding
            var containsBinding =
                control.QueryParameters.RawValues.Any(p => p.Value is IValueBinding) ||
                control.Params.RawValues.Any(p => p.Value is IValueBinding) ||
                control.GetValueRaw(RouteLink.UrlSuffixProperty) is IValueBinding;

            if (containsBinding)
            {
                var group = new KnockoutBindingGroup();
                group.Add("href", GenerateKnockoutHrefExpression(control.RouteName, control, context));
                writer.AddKnockoutDataBind("attr", group);
            }

            if (control.RenderOnServer || !containsBinding)
            {
                writer.AddAttribute("href", EvaluateRouteUrl(control.RouteName, control, context));
            }
        }

        public static string EvaluateRouteUrl(string routeName, RouteLink control, IDotvvmRequestContext context)
        {
            var urlSuffix = GenerateUrlSuffixCore(control.GetValue(RouteLink.UrlSuffixProperty) as string, control);
            var coreUrl = GenerateRouteUrlCore(routeName, control, context) + urlSuffix;

            if ((bool)control.GetValue(Internal.IsSpaPageProperty)! && !(bool)control.GetValue(Internal.UseHistoryApiSpaNavigationProperty)!)
            {
                return "#!/" + (coreUrl.StartsWith("~/", StringComparison.Ordinal) ? coreUrl.Substring(2) : coreUrl);
            }
            else
            {
                return context.TranslateVirtualPath(coreUrl);
            }
        }

        private static string GenerateRouteUrlCore(string routeName, RouteLink control, IDotvvmRequestContext context)
        {
            var route = GetRoute(context, routeName);
            var parameters = ComposeNewRouteParameters(control, context, route);

            // evaluate bindings on server
            foreach (var param in parameters.Where(p => p.Value is IStaticValueBinding).ToList())
            {
                EnsureValidBindingType((IBinding)param.Value);
                parameters[param.Key] = ((IValueBinding)param.Value).Evaluate(control);   // TODO: see below
            }

            // generate the URL
            return route.BuildUrl(parameters);
        }

        private static string GenerateUrlSuffixCore(string? urlSuffix, RouteLink control)
        {
            // generate the URL suffix
            return UrlHelper.BuildUrlSuffix(urlSuffix, control.QueryParameters);
        }

        private static RouteBase GetRoute(IDotvvmRequestContext context, string routeName)
        {
            return context.Configuration.RouteTable[routeName];
        }

        public static string GenerateKnockoutHrefExpression(string routeName, RouteLink control, IDotvvmRequestContext context)
        {
            var link = GenerateRouteLinkCore(routeName, control, context);

            var urlSuffix = GetUrlSuffixExpression(control);
            if ((bool)control.GetValue(Internal.IsSpaPageProperty)! && !context.Configuration.UseHistoryApiSpaNavigation)
            {
                return $"'#!/' + {link}{(urlSuffix == null ? "" : " + " + urlSuffix)}";
            }
            else
            {
                return $"'{context.TranslateVirtualPath("~/")}' + {link}{(urlSuffix == null ? "" : " + " + urlSuffix)}";
            }
        }

        private static string? GetUrlSuffixExpression(RouteLink control)
        {
            var urlSuffixBase =
                control.GetValueBinding(RouteLink.UrlSuffixProperty)
                ?.Apply(binding => binding.GetKnockoutBindingExpression(control))
                ?? JsonConvert.SerializeObject(control.UrlSuffix ?? "");
            var queryParams =
                control.QueryParameters.RawValues.Select(p => TranslateRouteParameter(control, p, true)).StringJoin(",");

            // generate the function call
            return
                queryParams.Length > 0 ? $"dotvvm.buildUrlSuffix({urlSuffixBase}, {{{queryParams}}})" :
                urlSuffixBase != "\"\"" ? urlSuffixBase :
                null;
        }

        private static string GenerateRouteLinkCore(string routeName, RouteLink control, IDotvvmRequestContext context)
        {
            var route = GetRoute(context, routeName);
            var parameters = ComposeNewRouteParameters(control, context, route);

            var parametersExpression = parameters.Select(p => TranslateRouteParameter(control, p)).StringJoin(",");
            // generate the function call

            return
                route.ParameterNames.Any()
                    ? $"dotvvm.buildRouteUrl({JsonConvert.ToString(route.Url)}, {{{parametersExpression}}})"
                    : JsonConvert.ToString(route.Url);
        }

        private static string TranslateRouteParameter<T>(DotvvmBindableObject control, KeyValuePair<string, T> param, bool caseSensitive = false)
        {
            string expression = "";
            if (param.Value is IBinding binding)
            {
                EnsureValidBindingType(binding);

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

        private static Dictionary<string, object?> ComposeNewRouteParameters(RouteLink control, IDotvvmRequestContext context, RouteBase route)
        {
            var parameters = new Dictionary<string, object?>(route.DefaultValues, StringComparer.OrdinalIgnoreCase);
            foreach (var param in context.Parameters!)
            {
                parameters[param.Key] = param.Value;
            }
            foreach (var item in control.Params.RawValues)
            {
                parameters[item.Key] = item.Value;
            }
            return parameters;
        }
    }
}
