#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Utils;
using System.Web.Routing;
using System.Web;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Adapters.WebForms.Controls
{
    public class WebFormsLinkUtils
    {
        public static HtmlGenericControl BuildWebFormsRouteLink(DotvvmControl container, IDotvvmRequestContext context, HtmlCapability htmlCapability, TextOrContentCapability textOrContent, RouteLinkCapability routeLinkCapability, Route webFormsRoute)
        {
            var link = new HtmlGenericControl("a", textOrContent, htmlCapability);

            var parameters = BuildParameters(context, routeLinkCapability, webFormsRoute);
            if (routeLinkCapability.UrlSuffix is { HasBinding: true, BindingOrDefault: IValueBinding }
                || routeLinkCapability.Params.Any(p => p.Value is { HasBinding: true, BindingOrDefault: IValueBinding }))
            {
                // bindings are used, we have to generate client-script code
                var fragments = new List<string> { KnockoutHelper.MakeStringLiteral(context.TranslateVirtualPath("~/")) };

                // generate binding and embed it in the function call
                var routeUrlExpression = GenerateRouteUrlExpression(container, webFormsRoute, parameters);
                fragments.Add(routeUrlExpression);

                // generate URL suffix
                if (GenerateUrlSuffixExpression(container, routeLinkCapability) is string urlSuffix)
                {
                    fragments.Add(urlSuffix);
                }

                // render the binding and try to evaluate it on the server
                link.AddAttribute("data-bind", "attr: { 'href': " + fragments.StringJoin("+") + "}");
                if (container.DataContext != null)
                {
                    try
                    {
                        var url = context.TranslateVirtualPath(EvaluateRouteUrl(container, webFormsRoute, parameters, routeLinkCapability));
                        link.SetAttribute("href", url);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            else
            {
                // the value can be built on the server
                var url = context.TranslateVirtualPath(EvaluateRouteUrl(container, webFormsRoute, parameters, routeLinkCapability));
                link.SetAttribute("href", url);
            }

            return link;
        }

        private static IDictionary<string, ValueOrBinding<object>> BuildParameters(IDotvvmRequestContext context, RouteLinkCapability routeLinkCapability, Route webFormsRoute)
        {
            var parameters = webFormsRoute.Defaults?.ToDictionary(t => t.Key, t => ValueOrBinding<object>.FromBoxedValue(t.Value))
                             ?? new Dictionary<string, ValueOrBinding<object>>();
            foreach (var param in context.Parameters)
            {
                parameters[param.Key] = ValueOrBinding<object>.FromBoxedValue(param.Value);
            }

            foreach (var item in routeLinkCapability.Params)
            {
                parameters[item.Key] = item.Value;
            }

            return parameters;
        }

        private static string EvaluateRouteUrl(DotvvmControl container, Route webFormsRoute, IDictionary<string, ValueOrBinding<object>> parameters, RouteLinkCapability routeLinkCapability)
        {
            // evaluate bindings on server
            var routeValues = new RouteValueDictionary();
            foreach (Match param in Regex.Matches(webFormsRoute.Url, @"\{([^{}/]+)\}"))       // https://referencesource.microsoft.com/#System.Web/Routing/RouteParser.cs,48
            {
                var paramName = param.Groups[1].Value;
                parameters.TryGetValue(paramName, out var value);
                routeValues[paramName] = value.Evaluate(container) ?? "";
            }

            // generate the URL
            return "~/"
                   + webFormsRoute.GetVirtualPath(HttpContext.Current.Request.RequestContext, routeValues)?.VirtualPath
                   + UrlHelper.BuildUrlSuffix(routeLinkCapability.UrlSuffix?.Evaluate(container), routeLinkCapability.QueryParameters.ToDictionary(p => p.Key, p => p.Value.Evaluate(container)));
        }

        private static string GenerateRouteUrlExpression(DotvvmControl container, Route webFormsRoute, IDictionary<string, ValueOrBinding<object>> parameters)
        {
            var parametersExpression = parameters
                .Select(p => $"{KnockoutHelper.MakeStringLiteral(p.Key)}: {p.Value.GetJsExpression(container)}")
                .OrderBy(p => p)
                .StringJoin(",");
            var routeUrlExpression = $"dotvvm.buildRouteUrl({KnockoutHelper.MakeStringLiteral(webFormsRoute.Url)}, {{{parametersExpression}}})";
            return routeUrlExpression;
        }

        private static string GenerateUrlSuffixExpression(DotvvmControl container, RouteLinkCapability routeLinkCapability)
        {
            var urlSuffixBase = routeLinkCapability.UrlSuffix?.GetJsExpression(container) ?? "\"\"";
            var queryParams = routeLinkCapability.QueryParameters
                .Select(p => $"{KnockoutHelper.MakeStringLiteral(p.Key.ToLowerInvariant())}: {p.Value.GetJsExpression(container)}")
                .OrderBy(p => p)
                .StringJoin(",");

            // generate the function call
            if (queryParams.Any())
            {
                return $"dotvvm.buildUrlSuffix({urlSuffixBase}, {{{queryParams}}})";
            }
            else if (urlSuffixBase != "\"\"")
            {
                return urlSuffixBase;
            }
            else
            {
                return null;
            }
        }
    }
}
#endif
