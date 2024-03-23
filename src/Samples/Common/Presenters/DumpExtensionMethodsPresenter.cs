using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DotVVM.Samples.Common.Presenters
{
    public class DumpExtensionMethodsPresenter : IDotvvmPresenter
    {
        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            var cache = context.Configuration.ServiceProvider.GetService<ExtensionMethodsCache>();

            var contents = typeof(ExtensionMethodsCache)
                .GetField("methodsCache", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(cache) as ConcurrentDictionary<string, ImmutableArray<MethodInfo>>;

            var dump = contents.SelectMany(p => p.Value.Select(m => new {
                Namespace = p.Key,
                m.Name,
                m.DeclaringType!.FullName,
                Params = m.GetParameters().Select(p => new {
                    p.Name,
                    Type = p.ParameterType!.FullName
                }),
                m.IsGenericMethodDefinition,
                GenericParameters = m.IsGenericMethodDefinition ? m.GetGenericArguments().Select(a => new {
                    a.Name
                }) : null
            }))
            .OrderBy(m => m.Namespace).ThenBy(m => m.Name);

            await context.HttpContext.Response.WriteAsync("ExtensionMethodsCache dump: " + JsonConvert.SerializeObject(dump, Formatting.Indented));
        }
    }
}
