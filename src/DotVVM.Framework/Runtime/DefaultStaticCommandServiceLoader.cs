using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Runtime
{
    public class DefaultStaticCommandServiceLoader : IStaticCommandServiceLoader
    {
        public virtual object GetStaticCommandService(Type serviceType, IDotvvmRequestContext context)
        {
            return context.Services.GetRequiredService(serviceType);
        }
    }
}
