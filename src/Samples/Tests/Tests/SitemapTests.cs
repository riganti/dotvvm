using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Samples.Tests
{
    public class SitemapTests : AppSeleniumTest
    {
        public SitemapTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Sitemap_Loads()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.Sitemap);
                Assert.Contains("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">", browser.Driver.PageSource);
            });
        }
    }
}
