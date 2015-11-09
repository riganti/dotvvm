using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.VS2015Extension.Common
{
    public class CommonWindsorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            var rootNamespace = typeof(CommonWindsorInstaller).Namespace;
            container.Register(Classes.FromThisAssembly().Where(w => w.Namespace.StartsWith(rootNamespace + ".LocalEnvironment")).LifestyleSingleton());
        }
    }
}