using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Configuration
{
    [TestClass]
    public class DotvvmConfigurationTests
    {

        [TestMethod]
        public void MultipleInitializationTest()
        {
            var config1 = DotvvmTestHelper.CreateConfiguration();

            new EnvironmentConfigurationInitializer().Initialize(config1, true);
            Assert.ThrowsException<InvalidOperationException>(() => {
                //another initialization need to throw exp.
                new EnvironmentConfigurationInitializer().Initialize(config1, false);
            });

            //another config 
            var config2 = DotvvmTestHelper.CreateConfiguration();
            var initializer = new EnvironmentConfigurationInitializer();
            initializer.Initialize(config2, true);

            //another initialization need to throw exp.
            Assert.ThrowsException<InvalidOperationException>(() => {
                initializer.Initialize(config2, false);
            });
            Assert.ThrowsException<InvalidOperationException>(() => {
                initializer.Initialize(config2, true);
            });
        }
    }
}
