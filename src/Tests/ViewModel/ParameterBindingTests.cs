using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.ViewModel
{
    [TestClass]
    public class ParameterBindingTests
    {
        private TestDotvvmRequestContext context;
        private AttributeViewModelParameterBinder binder;

        [TestInitialize]
        public void Init()
        {
            context = new TestDotvvmRequestContext()
            {
                Parameters = new Dictionary<string, object>(),
                Query = new TestQueryCollection()
            };
            binder = new AttributeViewModelParameterBinder();
        }

        [TestMethod]
        public void ParameterBinding_ParameterExists()
        {
            context.Parameters["route1"] = "test";

            var viewModel = new TestViewModelRouteString();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual("test", viewModel.Route1);
        }
        
        [TestMethod]
        public void ParameterBinding_ParameterExists_IntToStringConversion()
        {
            context.Parameters["route1"] = 15;

            var viewModel = new TestViewModelRouteString();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual("15", viewModel.Route1);
        }

        [TestMethod]
        public void ParameterBinding_ParameterExists_StringToIntConversion()
        {
            context.Parameters["route1"] = "15";

            var viewModel = new TestViewModelRouteInt();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual(15, viewModel.Route1);
        }

        [TestMethod]
        public void ParameterBinding_ParameterExists_StringToNullableIntConversion()
        {
            context.Parameters["route1"] = "15";

            var viewModel = new TestViewModelRouteNullableInt();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual(15, viewModel.Route1);
        }

        [TestMethod]
        public void ParameterBinding_ParameterExists_StringToEnumConversion()
        {
            ((Dictionary<string, string>)context.Query)["q"] = "Two";

            var viewModel = new TestViewModelQueryEnum();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual(TestEnum.Two, viewModel.Q);
        }
        
        [TestMethod]
        public void ParameterBinding_ParameterNotExists()
        {
            context.Parameters["route2"] = "test";

            var viewModel = new TestViewModelRouteString();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual(null, viewModel.Route1);
        }

        [TestMethod]
        public void ParameterBinding_ParameterExists_InvalidNumberFormat()
        {
            context.Parameters["route1"] = "test";

            var viewModel = new TestViewModelRouteInt();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual(0, viewModel.Route1);
        }

        [TestMethod]
        public void ParameterBinding_MultipleParameters()
        {
            context.Parameters["A"] = "24";
            context.Parameters["B"] = "True";
            context.Parameters["C"] = "abc";

            var viewModel = new TestViewModelMultipleParameters();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual(24, viewModel.A);
            Assert.AreEqual(true, viewModel.B);
            Assert.AreEqual("abc", viewModel.C);
        }


        [TestMethod]
        public void ParameterBinding_MultipleParameters_Inherited()
        {
            context.Parameters["A"] = "24";
            context.Parameters["B"] = "True";
            context.Parameters["C"] = "abc";

            var viewModel = new TestViewModelMultipleParametersInherited();
            binder.BindParameters(context, viewModel);

            Assert.AreEqual(24, viewModel.A);
            Assert.AreEqual(true, viewModel.B);
            Assert.AreEqual("abc", viewModel.C);
        }


        public class TestViewModelRouteString
        {

            [FromRoute("route1")]
            public string Route1 { get; set; }

        }

        public class TestViewModelRouteInt
        {

            [FromRoute("route1")]
            public int Route1 { get; set; }

        }

        public class TestViewModelRouteNullableInt
        {

            [FromRoute("route1")]
            public int? Route1 { get; set; }

        }


        public class TestViewModelQueryEnum
        {

            [FromQuery("q")]
            public TestEnum Q { get; set; }

        }

        public class TestViewModelMultipleParameters
        {
            [FromRoute("A")]
            public int A { get; set; }

            [FromRoute("B")]
            public bool B { get; set; }

            [FromRoute("C")]
            public string C { get; private set; }
        }

        public enum TestEnum
        {
            Zero = 0,
            One = 1,
            Two = 2,
            Three = 3
        }

        public class TestViewModelMultipleParametersInherited : TestViewModelMultipleParameters
        {
            
        }
    }
}
