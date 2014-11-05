using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Routing;

namespace Redwood.Framework.Tests.Routing
{
    [TestClass]
    public class RedwoodRouteTests
    {


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedwoodRoute_IsMatch_RouteMustNotStartWithSlash()
        {
            var route = new RedwoodRoute("/Test", null, null, null);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RedwoodRoute_IsMatch_RouteMustNotEndWithSlash()
        {
            var route = new RedwoodRoute("Test/", null, null, null);
        }

        [TestMethod]
        public void RedwoodRoute_IsMatch_EmptyRouteMatchesEmptyUrl()
        {
            var route = new RedwoodRoute("", null, null, null);
            
            IDictionary<string, object> parameters;
            var result = route.IsMatch("", out parameters);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RedwoodRoute_IsMatch_UrlWithoutParametersExactMatch()
        {
            var route = new RedwoodRoute("Hello/Test/Page.txt", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Hello/Test/Page.txt", out parameters);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RedwoodRoute_IsMatch_UrlWithoutParametersNoMatch()
        {
            var route = new RedwoodRoute("Hello/Test/Page.txt", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Hello/Test/Page", out parameters);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RedwoodRoute_IsMatch_UrlTwoParametersBothSpecified()
        {
            var route = new RedwoodRoute("Article/{Id}/{Title}", null, new { Title = "test" }, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/15/Test-title", out parameters);

            Assert.IsTrue(result);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("15", parameters["Id"]);
            Assert.AreEqual("Test-title", parameters["Title"]);
        }
        
        [TestMethod]
        public void RedwoodRoute_IsMatch_UrlTwoParametersOneSpecifiedOneDefault()
        {
            var route = new RedwoodRoute("Article/{Id}/{Title}", null, new { Title = "test" }, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/15", out parameters);

            Assert.IsTrue(result);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("15", parameters["Id"]);
            Assert.AreEqual("test", parameters["Title"]);
        }
        
        [TestMethod]
        public void RedwoodRoute_IsMatch_UrlTwoParametersBothRequired_NoMatchWhenOneSpecified()
        {
            var route = new RedwoodRoute("Article/{Id}/{Title}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/15", out parameters);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RedwoodRoute_IsMatch_UrlTwoParametersBothRequired_DifferentPart()
        {
            var route = new RedwoodRoute("Article/id_{Id}/{Title}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Articles/id_15", out parameters);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RedwoodRoute_IsMatch_UrlTwoParametersBothRequired_BothSpecified()
        {
            var route = new RedwoodRoute("Article/id_{Id}/{Title}", null, null, null);

            IDictionary<string, object> parameters;
            var result = route.IsMatch("Article/id_15/test", out parameters);

            Assert.IsTrue(result);
            Assert.AreEqual(2, parameters.Count);
            Assert.AreEqual("15", parameters["Id"]);
            Assert.AreEqual("test", parameters["Title"]);
        }

        [TestMethod]
        public void RedwoodRoute_BuildUrl_UrlTwoParameters()
        {
            var route = new RedwoodRoute("Article/id_{Id}/{Title}", null, null, null);

            var result = route.BuildUrl(new { Id = 15, Title = "Test" });

            Assert.AreEqual("Article/id_15/Test", result);
        }
    }
}
