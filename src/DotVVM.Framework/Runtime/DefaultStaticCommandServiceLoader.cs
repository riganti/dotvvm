using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Runtime
{
    [Obsolete(DefaultStaticCommandServiceLoader.DeprecationNotice)]
    public class DefaultStaticCommandServiceLoader : IStaticCommandServiceLoader
    {
        public const string DeprecationNotice = "IStaticCommandServiceLoader is an only temporary workaround for a flaw in service registration in DotVVM.Hosting.OWIN and it will be REMOVED SOON!. " +
                                                "\n\rNow you can replace IServiceProvider by your own in Startup class by using optional parameter 'Func<IServiceConllection, IServiceProvider> serviceProviderFactoryMethod' in app.UseDotVVM() method.";
        public virtual object GetStaticCommandService(Type serviceType, IDotvvmRequestContext context)
        {
            return context.Services.GetRequiredService(serviceType);
        }

        public virtual void DisposeStaticCommandServices(IDotvvmRequestContext context)
        {
        }
    }
}
