using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Common.Routing
{
    [TestClass]
    public class UrlHelperTests
    {

        [DataTestMethod]
        [DataRow("/local", true)]
        [DataRow("~/local", true)]
        [DataRow("//local", false)]
        [DataRow("~//local", false)]
        [DataRow("https://www.google.com", false)]
        public void UrlHelper_IsLocalUrl(string url, bool exepectedResult)
        {
            var result = UrlHelper.IsLocalUrl(url);
            Assert.AreEqual(exepectedResult, result);
        }

    }
}
