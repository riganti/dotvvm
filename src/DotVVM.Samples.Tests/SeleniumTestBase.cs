using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Samples.Tests
{
    public class SeleniumTestBase
    {
        public TestContext TestContext { get; set; }
        private WebDriverFacotry factory;

        private WebDriverFacotry Factory => factory ?? (factory = new WebDriverFacotry());

        protected virtual List<Func<IWebDriver>> BrowserFactories => Factory.GetDriverFactories();

        /// <summary>
        /// Runs the specified action in all configured browsers.
        /// </summary>
        protected void RunInAllBrowsers(Action<SeleniumBrowserHelper> action)
        {
            if (BrowserFactories.Count == 0)
            {
                throw new Exception("Factory doesn't contains drivers! Enable one driver at least to start UI Tests!");
            }
            foreach (var browserFactory in BrowserFactories)
            {
                var attemptNumber = 0;
                string browserName;
                Exception exception;
                do
                {
                    attemptNumber++;
                    exception = null;
                    var browser = browserFactory();
                    browserName = browser.GetType().Name;
                    var helper = new SeleniumBrowserHelper(browser);

                    try
                    {
                        action(helper);
                    }
                    catch (Exception ex)
                    {
                        // make screenshot
                        try
                        {
                            var filename = Path.Combine(TestContext.TestDeploymentDir, "fail" + attemptNumber + ".png");
                            helper.TakeScreenshot(filename);
                            TestContext.AddResultFile(filename);
                        }
                        catch
                        {
                        }

                        // fail the test
                        exception = ex;
                    }
                    finally
                    {
                        helper.Dispose();
                    }
                }
                while (exception != null && attemptNumber == 1);

                if (exception != null)
                {
                    throw new Exception(string.Format("Test failed in browser {0}.", browserName), exception);
                }
            }
        }
    }
}