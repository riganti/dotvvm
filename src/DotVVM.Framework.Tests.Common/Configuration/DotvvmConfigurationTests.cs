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

            DotvvmConfigurationEnvironmentInitializer.Initialize(config1, true);
            Assert.ThrowsException<InvalidOperationException>(() => {
                //another initialization need to throw exp.
                DotvvmConfigurationEnvironmentInitializer.Initialize(config1, false);
            });

            //another config 
            var config2 = DotvvmTestHelper.CreateConfiguration();
            DotvvmConfigurationEnvironmentInitializer.Initialize(config2, true);

            //another initialization need to throw exp.
            Assert.ThrowsException<InvalidOperationException>(() => {
                DotvvmConfigurationEnvironmentInitializer.Initialize(config2, false);
            });
            Assert.ThrowsException<InvalidOperationException>(() => {
                DotvvmConfigurationEnvironmentInitializer.Initialize(config2, true);
            });
        }
    }
}
