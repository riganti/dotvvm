#if  NET461
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Compiler.Exceptions;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;

namespace DotVVM.Compiler.Resolving
{
    public class OwinStartupClassResolver
    {
        private Type ResolveOwinStartupClassType(Assembly assembly)
        {
            var attributes = assembly.GetCustomAttributes();
            var owinStartupAttributes = attributes.Where(s => s.GetType().Name == typeof(OwinStartupAttribute).Name).ToList();

            if (owinStartupAttributes.Count > 1)
            {

            }
            if (owinStartupAttributes.Count < 1)
            {
                throw new ConfigurationInitializationException("Compiler cannot find startup class marked by Microsoft.Owin.OwinStartupAttribute.");
            }
            //needed if project reference different version of owin then compiler
            var owinAttribute = owinStartupAttributes.First();
            var owinAttributeType = owinAttribute .GetType();
            var startupTypeProperty = owinAttributeType.GetProperty(nameof(OwinStartupAttribute.StartupType));
            return startupTypeProperty.GetValue(owinAttribute) as Type;
        }

        private List<MethodInfo> ResolveConfigureServicesMethods(Type owinStartupType)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            methods.Add(owinStartupType.GetMethod("ConfigureDotvvmServices", new[] { typeof(IDotvvmOptions) }));
            methods.Add(owinStartupType.GetMethod("ConfigureServices", new[] { typeof(IDotvvmOptions) }));
            methods = methods.Where(s => s != null).ToList();

            if (!methods.Any())
            {
                throw new ConfigurationInitializationException("Method void ConfigureOptions(IDotvvmOptions options) in OWIN startup class is missing. Please follow docs https://www.dotvvm.com/docs/owin-compiler.");
            }
            return methods;
        }

        public IServicesStartupClassExecutor GetServiceConfigureExecutor(Assembly assembly)
        {
            var owinStartupType = ResolveOwinStartupClassType(assembly);
            var resolvedMethods = ResolveConfigureServicesMethods(owinStartupType);

            return new ServicesStartupClassExecutor(resolvedMethods.FirstOrDefault(), owinStartupType);
        }

    }
}
#endif
