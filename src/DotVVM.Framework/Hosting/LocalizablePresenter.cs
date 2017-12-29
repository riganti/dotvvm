using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting
{
    /// A DotVVM Presenter that reads culture by <see cref="getCulture" />,
    /// sets the thread culture and invokes the default dotvvm presenter (obtained from IServiceProvider)
    public class LocalizablePresenter : IDotvvmPresenter
    {
        private readonly Func<IDotvvmRequestContext, Task> nextPresenter;
        private readonly Func<IDotvvmRequestContext, CultureInfo> getCulture;

        public LocalizablePresenter(
            Func<IDotvvmRequestContext, CultureInfo> getCulture,
            Func<IDotvvmRequestContext, Task> nextPresenter
        )
        {
            this.getCulture = getCulture;
            this.nextPresenter = nextPresenter;
        }

        public Task ProcessRequest(IDotvvmRequestContext context)
        {
            var culture = this.getCulture(context);
            if (culture != null)
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
            return this.nextPresenter(context);
        }

        /// Creates a <see cref="LocalizablePresenter" /> factory that read the culture from route parameter.
        /// <param name="name">Name of the route parameter</param>
        /// <param name="redirectWhenNotFound">If the culture is invalid, it will perform redirect to a url with default culture specified.</param>
        public static Func<IServiceProvider, LocalizablePresenter> BasedOnParameter(string name, bool redirectWhenNotFound = true)
        {
            void redirect(IDotvvmRequestContext context)
            {
                var routeParameters = context.Parameters.ToDictionary(e => e.Key, e => e.Value);
                if (context.Configuration.DefaultCulture.Equals(routeParameters[name]))
                    throw new Exception($"The specified default culture is probably invalid");
                routeParameters[name] = context.Configuration.DefaultCulture;
                context.RedirectToRoute(context.Route.RouteName, routeParameters, query: context.HttpContext.Request.Query);
            }
            CultureInfo getCulture(IDotvvmRequestContext context) =>
                context.Parameters.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value as string) ? CultureInfo.GetCultureInfo((string)value) : null;
            var presenter = new LocalizablePresenter(
                redirectWhenNotFound ? WithRedirectOnFailure(redirect, getCulture) : getCulture,
                context => context.Services.GetRequiredService<IDotvvmPresenter>().ProcessRequest(context)
            );
            return _ => presenter;
        }

        /// Creates a <see cref="LocalizablePresenter" /> factory that read the culture from request query string parameter.
        /// <param name="name">Name of the query string parameter</param>
        /// <param name="redirectWhenNotFound">If the culture is invalid, it will perform redirect to a url with default culture specified.</param>
        public static Func<IServiceProvider, LocalizablePresenter> BasedOnQuery(string name, bool redirectWhenNotFound = true)
        {
            void redirect(IDotvvmRequestContext context)
            {
                var url = new UriBuilder(context.HttpContext.Request.Url);
                url.Query =
                    context.HttpContext.Request.Query
                    .Where(q => q.Key != name)
                    .Concat(new [] { new KeyValuePair<string, string>(name, context.Configuration.DefaultCulture) })
                    .Select(q => Uri.EscapeUriString(q.Key) + "=" + Uri.EscapeUriString(q.Value)).Apply(s => string.Join("&", s));
                if (url.ToString() == context.HttpContext.Request.Url.ToString())
                    throw new Exception($"The specified default culture is probably invalid");
                context.RedirectToUrl(url.ToString());
            }
            CultureInfo getCulture(IDotvvmRequestContext context) =>
                context.Query.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value) ? CultureInfo.GetCultureInfo(value) : null;
            var presenter = new LocalizablePresenter(
                redirectWhenNotFound ? WithRedirectOnFailure(redirect, getCulture) : getCulture,
                context => context.Services.GetRequiredService<IDotvvmPresenter>().ProcessRequest(context)
            );
            return _ => presenter;
        }

        /// Wraps cultureGetter with error handling. It calls the doRedirect on errors, and expects it to throw and exception (probably <see cref="DotvvmInterruptRequestExecutionException"/>)
        public static Func<IDotvvmRequestContext, CultureInfo> WithRedirectOnFailure(Action<IDotvvmRequestContext> doRedirect, Func<IDotvvmRequestContext, CultureInfo> cultureGetter) =>
            context => {
                try
                {
                    var result = cultureGetter(context);
                    if (result?.LCID == 4096)
                        doRedirect(context); // it seems that when a culture does not exists, the constucotr throws an exception on Mono, but returns an instance on .NET Core (with LCID = 4096)
                    return result;
                }
                catch(CultureNotFoundException)
                {
                    doRedirect(context);
                    Debug.Assert(false);
                    throw;
                }
            };
    }
}