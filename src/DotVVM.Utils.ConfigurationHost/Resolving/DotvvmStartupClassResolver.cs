using System;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using DotVVM.Utils.ConfigurationHost;

namespace DotVVM.Compiler.Resolving
{
    public class DotvvmStartupClassResolver
    {

        public IDotvvmStartup GetDotvvmStartupInstance(Assembly assembly)
        {
            //find all implementations of IDotvvmStartup
            var dotvvmStartups = assembly.GetLoadableTypes().Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToArray();

            
            if (dotvvmStartups.Length > 1)
            {
                throw new ConfigurationInitializationException($"Found more than one implementation of IDotvvmStartup ({string.Join(", ", dotvvmStartups.Select(s => s.Name)) }).");
            }
            if(dotvvmStartups.Length <= 0)
            {
                throw new ConfigurationInitializationException($"Could not found implementation of IDotvvmStartup in assembly {assembly.FullName}");
            }

            //create instance 
            var dotvvmStartup = dotvvmStartups.SingleOrDefault()?.Apply(Activator.CreateInstance).CastTo<IDotvvmStartup>();
            return dotvvmStartup;
        }

    }
}
