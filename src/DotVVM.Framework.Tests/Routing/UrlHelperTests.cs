using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Routing
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
        [DataRow("/https://www.google.com", true)]
        [DataRow("/\r\n/google.com", false)]
        [DataRow("/\n/google.com", false)]
        [DataRow("mailto:a", false)]
        [DataRow("/mailto:a", true)]
        [DataRow(@"\\www.google.com", false)] // Chrome replaces backslashes with forward slashes...
        [DataRow(@"\/www.google.com", false)]
        [DataRow(@"/\www.google.com", false)]
        [DataRow(@"/4aef74ba-388c-4292-9d53-98387e4f797b/reservation?LocationId=e5eed4c5-dfe9-45fd-a341-7408205d76ce&BeginDate=201909011300&Duration=2", true)]
        public void UrlHelper_IsLocalUrl(string url, bool exepectedResult)
        {
            var result = UrlHelper.IsLocalUrl(url);
            Assert.AreEqual(exepectedResult, result);
        }

    }
}
