using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.ResourceManagement
{
    public abstract class LocalResourceLocation : ILocalResourceLocation
    {
        public string GetUrl(IDotvvmRequestContext context, string name) =>
            context.Services.GetRequiredService<ILocalResourceUrlManager>().GetResourceUrl(this, context, name);

        public abstract Stream LoadResource(IDotvvmRequestContext context);
    }
}
