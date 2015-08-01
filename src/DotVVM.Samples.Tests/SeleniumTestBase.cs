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