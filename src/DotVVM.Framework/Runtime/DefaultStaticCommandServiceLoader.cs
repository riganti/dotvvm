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
        public const string DeprecationNotice = "IStaticCommandServiceLoader is only temporary workaround for a flaw in service registration in DotVVM.Hosting.OWIN and it will be removed soon. If you need to use it, you are doing something wrong.\n\nSee discussion at https://github.com/riganti/dotvvm/commit/46f043d28f5bda2f83f2bf827c65a2c32d56e252 and https://github.com/riganti/dotvvm/commit/3036726a9783bb953ddf1b7b9a62f2c27d0db7b3 for some context.";
        public virtual object GetStaticCommandService(Type serviceType, IDotvvmRequestContext context)
        {
            return context.Services.GetRequiredService(serviceType);
        }

        public virtual void DisposeStaticCommandServices(IDotvvmRequestContext context)
        {
        }
    }
}
