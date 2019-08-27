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
        public const string DeprecationNotice = @"IStaticCommandServiceLoader is a workaround for the case when you need to resolve the static command service from your IoC/DI container but you do not want to integrate it with IServiceProvider (for example if you integrate DotVVM in an existing ASP.NET project).
        In most cases, you don't want to use this method, as you can replace the IServiceProvider with your own implementation in Startup.cs by using optional parameter 'Func<IServiceConllection, IServiceProvider> serviceProviderFactoryMethod' of the app.UseDotVVM() method.
            See https://www.dotvvm.com/docs/tutorials/advanced-ioc-di-container-owin/2.0";
        public virtual object GetStaticCommandService(Type serviceType, IDotvvmRequestContext context)
        {
            return context.Services.GetRequiredService(serviceType);
        }

        public virtual void DisposeStaticCommandServices(IDotvvmRequestContext context)
        {
        }
    }
}
