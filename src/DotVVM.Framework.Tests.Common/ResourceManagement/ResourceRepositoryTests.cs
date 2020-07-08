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
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();
                configuration.Resources.Register("one", new NullResource { Dependencies = new[] { "two" } });
                configuration.Resources.Register("two", new NullResource { Dependencies = new[] { "one" } });
                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("one");
            });
        }

        [TestMethod]
        public void ResourceRepository_CyclicDependency2_Throws()
        {
            Assert.ThrowsException<DotvvmResourceException>(() => {
                var configuration = DotvvmConfiguration.CreateDefault();

                configuration.Resources.Register("three", new ScriptResource {
                    Location = new FileResourceLocation("C:\\Test\\three.js"),
                    Dependencies = new[] { "two" }
                });
                configuration.Resources.Register("two", new ScriptResource {
                    Location = new FileResourceLocation("C:\\Test\\two.js"),
                    Dependencies = new[] { "three" }
                });
                configuration.Resources.Register("one", new ScriptResource {
                    Location = new FileResourceLocation("C:\\Test\\one.js"),
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

                var manager = configuration.ServiceProvider.GetRequiredService<ResourceManager>();
                manager.AddRequiredResource("one");
            });
        }
    }
}
