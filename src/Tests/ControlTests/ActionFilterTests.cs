using System;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class ActionFilterTests
    {
        ControlTestHelper cth = new ControlTestHelper(config: config => {
        });
        OutputChecker check = new OutputChecker("testoutputs");

        [TestMethod]
        public async Task ActionFiltersOnViewModel()
        {
            var r = await cth.RunPage(typeof(DerivedViewModel), @"");

            Assert.IsTrue(r.CustomProperties.ContainsKey("A-OnViewModelCreatedAsync"));
            Assert.IsTrue(r.CustomProperties.ContainsKey("B-OnViewModelCreatedAsync"));
        }

        
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class TestActionFilter: ActionFilterAttribute
        {
            public TestActionFilter(string name)
            {
                Name = name;
            }

            public string Name { get; }
            protected internal override Task OnViewModelCreatedAsync(IDotvvmRequestContext context)
            {
                context.CustomResponseProperties.Add(Name + "-OnViewModelCreatedAsync", true);
                return Task.CompletedTask;
            }
        }

        [TestActionFilter("A")]
        public class BasicTestViewModel: DotvvmViewModelBase { }

        [TestActionFilter("B")]
        public class DerivedViewModel: BasicTestViewModel { }
    }
}
