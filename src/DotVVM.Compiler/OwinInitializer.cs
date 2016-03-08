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

            if (dotvvmStartups.Length == 0) throw new Exception("Could not find any implementation of IDotvvmStartup.");
            if (dotvvmStartups.Length > 1) throw new Exception($"Found more than one implementation of IDotvvmStartup ({string.Join(", ", dotvvmStartups.Select(s => s.Name)) }).");

            var startup = (IDotvvmStartup)Activator.CreateInstance(dotvvmStartups[0]);
            var config = OwinExtensions.CreateConfiguration(rootPath);
            startup.Configure(config, rootPath);
            config.CompiledViewsAssemblies = null;
            return config;
        }
    }
}
