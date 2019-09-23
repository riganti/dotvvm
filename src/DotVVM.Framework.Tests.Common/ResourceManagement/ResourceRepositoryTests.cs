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

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_1()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("7", new NullResource { Dependencies = new[] { "6", "5" } });

                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "4", "3" } });
                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "4", "3" } });

                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "2", "1" } });
                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "1" } });

                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "dotvvm" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "dotvvm" } });

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("7");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_2_Throws()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("7", new NullResource { Dependencies = new[] { "6", "5" } });

                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "4", "3" } });
                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "4", "3" } });

                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "2", "1" } });
                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "1" } });

                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "DotVVM" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "DotVVM" } });

                configuration.Resources.Register("DotVVM", new NullResource { Dependencies = new[] { "7" } }); ;//cyclic

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("7");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_3_Throws()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("8", new NullResource { Dependencies = new[] { "7", "6" } });

                configuration.Resources.Register("7", new NullResource { Dependencies = new[] { "5", "4" } });
                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "2" } });

                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "3" } });
                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "2" } });

                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "2", "1" } });

                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "5", "1" } });// cyclic

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("8");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_4_Throws()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "5" } });
                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "4" } });
                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "3" } });
                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "2" } });
                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "1" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "6" } });//cyclic

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("6");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_5_Throws()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();
                
                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "2" } });
                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "1" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "3" } });//cyclic

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("3");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_6()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("7", new NullResource { Dependencies = new[] { "5" } });

                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "4", "3" } });
                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "3", "1" } });

                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "1" } });
                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "2" } });

                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "dotvvm" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "dotvvm" } });

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("3");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_7()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "7", "4", "3" } });

                configuration.Resources.Register("7", new NullResource { Dependencies = new[] { "5" } });
                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "3", "1" } });

                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "1" } });
                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "2" } });

                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "dotvvm" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "dotvvm" } });

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("3");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_8_Throws()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "7", "4" } });

                configuration.Resources.Register("7", new NullResource { Dependencies = new[] { "5" } });
                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "3", "1" } });

                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "1" } });
                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "6", "2" } }); //cyclis

                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "dotvvm" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "dotvvm" } });

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("3");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency_Structure_9()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("10", new NullResource { Dependencies = new[] { "9", "8", "7" } });

                configuration.Resources.Register("9", new NullResource { Dependencies = new[] { "6", "5" } });
                configuration.Resources.Register("8", new NullResource { Dependencies = new[] { "5" } });
                configuration.Resources.Register("7", new NullResource { Dependencies = new[] { "5","4" } });

                configuration.Resources.Register("6", new NullResource { Dependencies = new[] { "3", "2" } });
                configuration.Resources.Register("5", new NullResource { Dependencies = new[] { "2" } });
                configuration.Resources.Register("4", new NullResource { Dependencies = new[] { "2", "1" } });

                configuration.Resources.Register("3", new NullResource { Dependencies = new[] { "dotvvm" } });
                configuration.Resources.Register("2", new NullResource { Dependencies = new[] { "dotvvm" } });
                configuration.Resources.Register("1", new NullResource { Dependencies = new[] { "dotvvm" } });

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("3");
            });
        }

    }
}
