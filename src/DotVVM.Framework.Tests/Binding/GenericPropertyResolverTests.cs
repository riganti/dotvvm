using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DotVVM.Framework.Tests.Binding.CommandResolverTests;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class GenericPropertyResolverTests
    {
        private DotvvmConfiguration configuration;
        private BindingCompilationService bindingService;

        [TestInitialize]
        public void Init()
        {
            this.configuration = DotvvmTestHelper.DefaultConfig;
            this.bindingService = configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
        }
        [TestMethod]
        public void ExpectedAsStringPropertyResolutionTest()
        {
            var binding = ValueBindingExpression.CreateBinding<int>(bindingService, vm => ((Test1)vm[0]).NumberToPass, (DataContextStack)null);
            var expression = binding.GetProperty<ExpectedAsStringBindingExpression>();
            Assert.IsNotNull(expression);
        }
    }
}
