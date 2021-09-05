using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Routing;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using System.Threading.Tasks;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Framework.Tests.Routing
{
    [TestClass]
    public class LocalizablePresenterTests
    {

        [TestMethod]
        public void LocalizablePresenter_RedirectsOnInvalidLanguageCode()
        {
            var config = DotvvmConfiguration.CreateDefault();
            var presenterFactory = LocalizablePresenter.BasedOnParameter("Lang");

            config.RouteTable.Add("Test", "test/Lang", "test", new { Lang = "en" }, presenterFactory);

            var context = DotvvmTestHelper.CreateContext(config);
            context.Parameters["Lang"] = "cz";
            context.Route = config.RouteTable.First();

            var httpRequest = new TestHttpContext();
            httpRequest.Request = new TestHttpRequest(httpRequest) { PathBase = "" };
            httpRequest.Request.Headers.Add(HostingConstants.SpaContentPlaceHolderHeaderName, new string[0]);

            context.HttpContext = httpRequest;
            var localizablePresenter = presenterFactory(config.ServiceProvider);

            Assert.ThrowsException<DotvvmInterruptRequestExecutionException>(() =>
                localizablePresenter.ProcessRequest(context));
        }


    }
}
