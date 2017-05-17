using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.ViewModel
{
    [TestClass]
    public class ModelStateFromExpressionTests
    {
        private readonly ViewModel viewModel;

        public ModelStateFromExpressionTests()
        {
            this.viewModel = new ViewModel();
            this.viewModel.Context = new TestDotvvmRequestContext {
                Configuration = DotvvmConfiguration.CreateDefault()
            };

            this.viewModel.Context.Configuration.ServiceLocator.GetService<IViewModelSerializationMapper>().Map(typeof(ViewModel), m => {
                m.Property(nameof(ViewModel.RenamedProperty)).Name = "rp";
            });
        }

        [TestMethod]
        public void ModelState_SinglePropertyExpression()
        {
            Assert.AreEqual("MyProperty", viewModel.CreateVmError(v => v.MyProperty, "").PropertyPath);
        }

        [TestMethod]
        public void ModelState_SingleRenamedProperty()
        {
            Assert.AreEqual("rp", viewModel.CreateVmError(v => v.RenamedProperty, "").PropertyPath);
        }

        [TestMethod]
        public void ModelState_NestedViewModelExpression()
        {
            Assert.AreEqual("AnotherProperty().StringProp", viewModel.CreateVmError(v => v.AnotherProperty.StringProp, "").PropertyPath);
        }

        [TestMethod]
        public void ModelState_ArrayAccessExpression()
        {
            Assert.AreEqual("AnotherProperty().TestViewModel2().Collection()[0]().StringValue", viewModel.CreateVmError(v => v.AnotherProperty.TestViewModel2.Collection[0].StringValue, "").PropertyPath);
        }

        class ViewModel: DotvvmViewModelBase
        {
            public int MyProperty { get; set; }
            public int RenamedProperty { get; set; }

            public TestViewModel AnotherProperty { get; set; }
        }
    }
}
