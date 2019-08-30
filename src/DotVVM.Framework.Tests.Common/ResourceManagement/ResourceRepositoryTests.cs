using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.ResourceManagement
{
    [TestClass]
    public class ResourceRepositoryTests
    {
        [TestMethod]
        public void ResourceRepository_CyclicDependency_Throws()
        {
            Assert.ThrowsException<DotvvmResourceException>(() =>
            {
                var configuration = DotvvmConfiguration.CreateDefault();
                configuration.Resources.Register("one", new NullResource { Dependencies = new[] { "two" } });
                configuration.Resources.Register("two", new NullResource { Dependencies = new[] { "one" } });
                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("one");
            });
        }
    }
}
