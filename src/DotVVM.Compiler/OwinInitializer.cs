using DotVVM.Framework;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Compiler
{
    class OwinInitializer
    {
        public static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, string rootPath)
        {
            var dotvvmStartups = webSiteAssembly.GetTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToArray();

            var startup = (IDotvvmStartup)Activator.CreateInstance(dotvvmStartups.Single());
            var config = OwinExtensions.CreateConfiguration(rootPath);
            startup.Configure(config, rootPath);
            config.CompiledViewsAssemblies = null;
            return config;
        }
    }
}
