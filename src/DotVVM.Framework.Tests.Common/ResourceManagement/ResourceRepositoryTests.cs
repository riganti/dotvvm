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
            var ex = Assert.ThrowsException<DotvvmResourceException>(() => {

                var configuration = DotvvmConfiguration.CreateDefault();
                configuration.Resources.Register("one", new NullResource { Dependencies = new[] { "two" } });
                configuration.Resources.Register("two", new NullResource { Dependencies = new[] { "one" } });

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("one");
            });
            Assert.AreEqual("Resource \"two\" has a cyclic dependency: two --> one --> two", ex.Message);
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency2_Throws()
        {
            var ex = Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();
 
                configuration.Resources.Register("one", new ScriptResource {
                    Location = new FileResourceLocation("C:\\Test\\one.js"),
                    Dependencies = new[] { "two" }
                });
                configuration.Resources.Register("two", new ScriptResource {
                    Location = new FileResourceLocation("C:\\Test\\two.js"),
                    Dependencies = new[] { "three" }
                });
                configuration.Resources.Register("three", new ScriptResource {
                    Location = new FileResourceLocation("C:\\Test\\three.js"),
                    Dependencies = new[] { "two" }
                });

                //When adding resource "one" inside of AssertAcyclicDependencies function dependancy of one is followed into "two". "two" is followed into its dependency "two"
                //Loop ending condition is resource ("one" in this case) == current (always "two" because "two" depends on "two") so the loop never ends.
                //My conclusion is that this check is faulty in case there is cyclic dependency not involving resource that is currently being added.
                //
                //                                                 <dependancy-k+n>  <-
                //                                                      V           <dependancy-k+1>
                //<resource being added> -> <dependancy-1> -> ... -> <dependancy-k> ^
                //
                //cycle k to k+n does not gent detected and results in an endless loop
            });
            Assert.AreEqual("Resource \"three\" has a cyclic dependency: three --> two --> three", ex.Message);
        }

        [TestMethod]
        public void ResourceRepository_CommonDependency_Ok()
        {
            var configuration = DotvvmConfiguration.CreateDefault();

            configuration.Resources.Register("common", new ScriptResource {
                Location = new FileResourceLocation("C:\\Test\\common.js"),
            });

            configuration.Resources.Register("up", new ScriptResource {
                Location = new FileResourceLocation("C:\\Test\\up.js"),
                Dependencies = new[] { "common" }
            });

            configuration.Resources.Register("down", new ScriptResource {
                Location = new FileResourceLocation("C:\\Test\\down.js"),
                Dependencies = new[] { "common" }
            });

            configuration.Resources.Register("start", new ScriptResource {
                Location = new FileResourceLocation("C:\\Test\\start.js"),
                Dependencies = new[] { "up", "down" }
            });

            var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
            manager.AddRequiredResource("start");
        }

        [TestMethod]
        public void ResourceRepository_SelfReference_Throws()
        {
            var ex = Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();
         
                configuration.Resources.Register("one", new NullResource {
                    Dependencies = new[] { "two" }
                });
                configuration.Resources.Register("two", new NullResource {
                    Dependencies = new[] { "two" }
                });
            });

            Assert.AreEqual("Resource \"two\" has a cyclic dependency: two --> two", ex.Message);
        }

        [TestMethod]
        public void ResourceRepository_SmallGraphCycle_Throws()
        {
            var ex = Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("six", new NullResource {
                });

                configuration.Resources.Register("one", new NullResource {
                    Dependencies = new[] { "two" }
                });

                configuration.Resources.Register("two", new NullResource {
                    Dependencies = new[] { "three", "six" }
                });

                configuration.Resources.Register("four", new NullResource {
                });

                configuration.Resources.Register("three", new NullResource {
                    Dependencies = new[] { "five", "four" }
                });

                configuration.Resources.Register("seven", new NullResource {
                });

                configuration.Resources.Register("five", new NullResource {
                    Dependencies = new[] { "seven", "two" }
                });
            });
            Assert.AreEqual("Resource \"five\" has a cyclic dependency: five --> two --> three --> five", ex.Message);
        }
    }
}
