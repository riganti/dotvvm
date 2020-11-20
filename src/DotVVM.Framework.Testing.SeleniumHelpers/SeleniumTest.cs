using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace DotVVM.Framework.Testing.SeleniumHelpers
{
    public class SeleniumTest
    {

        public void RunInAllBrowsers(Action<IWebDriver> test)
        {
            var browser = new ChromeDriver();
            test(browser);
        }

    }
}
