using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.IO;
using System.Linq;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;

namespace DotVVM.Samples.Tests
{
    public class SeleniumTestBase
    {

        public TestContext TestContext { get; set; }


        protected virtual Func<IWebDriver>[] BrowserFactories
        {
            get
            {
                return new Func<IWebDriver>[] {
                    () => new InternetExplorerDriver(),
                    //() => new FirefoxDriver(new FirefoxBinary(@"C:\Program Files (x86)\Mozilla Firefox\Firefox.exe"), new FirefoxProfile(), TimeSpan.FromSeconds(30)),
                    () => new ChromeDriver()
                };
            }
        }

        /// <summary>
        /// Runs the specified action in all configured browsers.
        /// </summary>
        protected void RunInAllBrowsers(Action<SeleniumBrowserHelper> action)
        {
            foreach (var browser in BrowserFactories.Select(f => f()))
            {
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
                        var filename = Path.Combine(TestContext.TestDeploymentDir, "fail.png");
                        helper.TakeScreenshot(filename);
                        TestContext.AddResultFile(filename);
                    }
                    catch
                    {
                    }

                    // fail the test
                    throw new Exception(string.Format("Test failed in browser {0}.", browser.GetType().Name), ex);
                }
                finally
                {
                    helper.Dispose();
                }
            }
        }


    }
}