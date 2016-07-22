using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Routing;

namespace DotVVM.Framework.Tests.Routing
{
    [TestClass]
    public class DotvvmRouteTests
    {


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DotvvmRoute_IsMatch_RouteMustNotStartWithSlash()
        {
            var route = new DotvvmRoute("/Test", null, null, null);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DotvvmRoute_IsMatch_RouteMustNotEndWithSlash()
        {
            var route = new DotvvmRoute("Test/", null, null, null);
        }

        [TestMethod]
        public void DotvvmRoute_IsMatch_EmptyRouteMatchesEmptyUrl()
        {
            var route = new DotvvmRoute("", null, null, null);
            
            IDictionary<string, object> parameters;
            var result = route.IsMatch("", out parameters);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlWithoutParametersExactMatch()
        {
            var route = new DotvvmRoute("Hello/Test/Page.txt", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Hello/Test/Page.txt", out parameters);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlWithoutParametersNoMatch()
        {
            var route = new DotvvmRoute("Hello/Test/Page.txt", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Hello/Test/Page", out parameters);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlTwoParametersBothSpecified()
        {
            var route = new DotvvmRoute("Article/{Id}/{Title}", null, new { Title = "test" }, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/15/Test-title", out parameters);

            Assert.IsTrue(result);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("15", parameters["Id"]);
            Assert.AreEqual("Test-title", parameters["Title"]);
        }
        
        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlTwoParametersOneSpecifiedOneDefault()
        {
            var route = new DotvvmRoute("Article/{Id}/{Title}", null, new { Title = "test" }, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/15", out parameters);

            Assert.IsTrue(result);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("15", parameters["Id"]);
            Assert.AreEqual("test", parameters["Title"]);
        }
        

        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlTwoParametersBothRequired_NoMatchWhenOneSpecified()
        {
            var route = new DotvvmRoute("Article/{Id}/{Title}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/15", out parameters);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlTwoParametersBothRequired_DifferentPart()
        {
            var route = new DotvvmRoute("Article/id_{Id}/{Title}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Articles/id_15", out parameters);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlOneParameterRequired_TwoSpecified()
        {
            var route = new DotvvmRoute("Article/{Id}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/15/test", out parameters);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DotvvmRoute_IsMatch_UrlTwoParametersBothRequired_BothSpecified()
        {
            var route = new DotvvmRoute("Article/id_{Id}/{Title}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/id_15/test", out parameters);

            Assert.IsTrue(result);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("15", parameters["Id"]);
            Assert.AreEqual("test", parameters["Title"]);
        }

        [TestMethod]
        public void DotvvmRoute_BuildUrl_UrlTwoParameters()
        {
            var route = new DotvvmRoute("Article/id_{Id}/{Title}", null, null, null);

            var result = route.BuildUrl(new { Id = 15, Title = "Test" });

            Assert.AreEqual("~/Article/id_15/Test", result);
        }

        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_OnePart()
        {
            var route = new DotvvmRoute("Article", null, null, null);

            var result = route.BuildUrl(new { });

            Assert.AreEqual("~/Article", result);
        }

        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts()
        {
            var route = new DotvvmRoute("Article/Test", null, null, null);

            var result = route.BuildUrl(new { });

            Assert.AreEqual("~/Article/Test", result);
        }

        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts_OptionalParameter_NoValue()
        {
            var route = new DotvvmRoute("Article/Test/{Id?}", null, null, null);

            var result = route.BuildUrl(new { });

            Assert.AreEqual("~/Article/Test", result);
        }


        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts_OptionalParameter_WithValue()
        {
            var route = new DotvvmRoute("Article/Test/{Id?}", null, null, null);

            var result = route.BuildUrl(new { Id = 5 });

            Assert.AreEqual("~/Article/Test/5", result);
        }

        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts_TwoParameters_OneOptional_NoValue()
        {
            var route = new DotvvmRoute("Article/Test/{Id}/{Id2?}", null, null, null);

            var result = route.BuildUrl(new { Id = 5 });

            Assert.AreEqual("~/Article/Test/5", result);
        }

        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts_TwoParameters_OneOptional_NoValue_Suffix()
        {
            var route = new DotvvmRoute("Article/Test/{Id}/{Id2?}/suffix", null, null, null);

            var result = route.BuildUrl(new { Id = 5 });

            Assert.AreEqual("~/Article/Test/5/suffix", result);
        }


        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts_TwoParameters_OneOptional_WithValue()
        {
            var route = new DotvvmRoute("Article/Test/{Id}/{Id2?}", null, null, null);

            var result = route.BuildUrl(new { Id = 5, Id2 = "aaa" });

            Assert.AreEqual("~/Article/Test/5/aaa", result);
        }



        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts_TwoParameters_OneOptional_WithValue_Suffix()
        {
            var route = new DotvvmRoute("Article/Test/{Id}/{Id2?}/suffix", null, null, null);

            var result = route.BuildUrl(new { Id = 5, Id2 = "aaa" });

            Assert.AreEqual("~/Article/Test/5/aaa/suffix", result);
        }



        [TestMethod]
        public void DotvvmRoute_BuildUrl_Static_TwoParts_TwoParameters_FirstOptionalOptional_Suffix()
        {
            var route = new DotvvmRoute("Article/Test/{Id?}/{Id2}/suffix", null, null, null);

            var result = route.BuildUrl(new { Id2 = "aaa" });

            Assert.AreEqual("~/Article/Test/aaa/suffix", result);
        }

        
        [TestMethod]
        public void DotvvmRoute_BuildUrl_CombineParameters_OneOptional()
        {
            var route = new DotvvmRoute("Article/{Id?}", null, null, null);

            var result = route.BuildUrl(new Dictionary<string, object>() { { "Id", 5 } }, new { });

            Assert.AreEqual("~/Article/5", result);
        }


        [TestMethod]
        public void DotvvmRoute_BuildUrl_ParameterOnly()
        {
            var route = new DotvvmRoute("{Id?}", null, null, null);

            var result = route.BuildUrl(new { });

            Assert.AreEqual("~/", result);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DotvvmRoute_BuildUrl_Invalid_UnclosedParameter()
        {
            var route = new DotvvmRoute("{Id", null, null, null);

            var result = route.BuildUrl(new { });
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DotvvmRoute_BuildUrl_Invalid_UnclosedParameterConstraint()
        {
            var route = new DotvvmRoute("{Id:int", null, null, null);

            var result = route.BuildUrl(new { });
        }


        [TestMethod]
        public void DotvvmRoute_BuildUrl_ParameterConstraint_Int()
        {
            var route = new DotvvmRoute("Article/id_{Id:int}/{Title}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/id_15/test", out parameters);

            Assert.IsTrue(result);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual(15, parameters["Id"]);
            Assert.AreEqual("test", parameters["Title"]);
        }

		[TestMethod]
		public void DotvvmRoute_Performace()
		{
			var route = new DotvvmRoute("Article/{name}@{domain}/{id:int}", null, null, null);

			IDictionary<string, object> parameters;
			Assert.IsFalse(route.IsMatch("Article/f" + new string('@', 2000) + "f/4f", out parameters));
		}
    }
}
