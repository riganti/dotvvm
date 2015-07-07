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
    }
}
