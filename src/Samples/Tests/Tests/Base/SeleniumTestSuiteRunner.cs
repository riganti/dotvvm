using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Configuration;
using Riganti.Selenium.Core.Drivers;
using Riganti.Selenium.Core.Drivers.Implementation;
using Riganti.Selenium.Core.Factories;
using Riganti.Selenium.Core.Factories.Implementation;
using Riganti.Selenium.xUnitIntegration;

namespace DotVVM.Samples.Tests.Base
{
    public class SeleniumTestSuiteRunner : XunitTestSuiteRunner
    {
        public SeleniumTestSuiteRunner(SeleniumTestsConfiguration configuration, TestContextProvider provider) : base(configuration, provider)
        {
        }

        protected override Dictionary<string, IWebBrowserFactory> CreateWebBrowserFactories()
        {
            var factories = base.CreateWebBrowserFactories();
            if (factories.TryGetValue("firefox:fast", out var original))
            {
                var factory = new FirefoxFactory(this);
                foreach (var option in original.Options)
                    factory.Options.Add(option.Key, option.Value);
                foreach (var capability in original.Capabilities)
                    factory.Capabilities.Add(capability);
                factories[factory.Name] = factory;
            }
            return factories;
        }

        private class FirefoxFactory : FirefoxFastWebBrowserFactory
        {
            public FirefoxFactory(TestSuiteRunner runner) : base(runner)
            {
            }

            protected override IWebBrowser CreateBrowser() => new FirefoxBrowser(this);
        }

        private class FirefoxBrowser : FirefoxFastWebBrowser
        {
            public FirefoxBrowser(LocalWebBrowserFactory factory) : base(factory)
            {
            }

            protected override IWebDriver CreateDriver()
            {
                var options = FirefoxHelpers.GetFirefoxOptions(Factory.Options);
                // Selenium's legacy profile uses startup.page=1, which now opens a privileged blanktab.html.
                options.SetPreference("browser.startup.page", 0);
                return new FirefoxDriver(FirefoxDriverService.CreateDefaultService(), options);
            }
        }
    }
}
