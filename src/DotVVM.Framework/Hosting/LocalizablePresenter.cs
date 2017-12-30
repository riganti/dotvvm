using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting
{
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
            {
#if DotNetCore
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
#else
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;
#endif
            }
            return this.nextPresenter(context);
        }

        public static Func<LocalizablePresenter> BasedOnParameter(string name)
        {
            var presenter = new LocalizablePresenter(
                context => context.Parameters.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value as string) ? new CultureInfo((string)value) : null,
                context => context.Services.GetRequiredService<IDotvvmPresenter>().ProcessRequest(context)
            );
            return () => presenter;
        }

        public static Func<LocalizablePresenter> BasedOnQuery(string name)
        {
            var presenter = new LocalizablePresenter(
                context => context.Query.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value as string) ? new CultureInfo((string)value) : null,
                context => context.Services.GetRequiredService<IDotvvmPresenter>().ProcessRequest(context)
            );
            return () => presenter;
        }
    }
}
