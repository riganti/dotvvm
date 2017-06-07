using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class SpaContentPlaceHolderTests : SeleniumTest
    {

        [TestMethod]
        public void Control_SpaContentPlaceHolder_SpaContentPlaceHolder()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_Default);
                browser.Wait(2000);

                // verify the URL after redirect to the DefaultRoute
                browser.CheckIfAlertTextEquals("javascript 2 resource loaded!");
                browser.ConfirmAlert();
                browser.Wait(2000);
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
                browser.ConfirmAlert();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB);
                
                // go to first page
                browser.First("a").Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageA + "/16");

                // test first page
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.ElementAt("span", 0).CheckIfTextEquals("3");

                // go to second page
                browser.FindElements("a").Single(l => l.GetText() == "Go to Task List").Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB);

                // try the task list
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);
                browser.Last("tr a").Click();
                browser.Wait();
                browser.Last(".table tr").CheckIfHasClass("completed");

                // test the browse back button
                browser.NavigateBack();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageA + "/16");

                // test first page
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.ElementAt("span", 0).CheckIfTextEquals("3");

                // test the forward button
                browser.NavigateForward();
                browser.Wait();

                // test the redirect inside SPA
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.Last("input[type=button]").Click();
                browser.Wait(2000);
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageA + "/15");

                // test the redirect outside SPA
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.FindElements("a").Single(l => l.GetText().Contains("Exit SPA")).Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ComplexSamples_TaskList_ServerRenderedTaskList);
            });
        }

        [TestMethod]
        public void Control_SpaContentPlaceHolder_SpaContentPlaceHolder_EnteredFromPageB()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB);
                browser.Wait(2000);

                // verify the URL after redirect to the DefaultRoute
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
                browser.ConfirmAlert();
                browser.Wait(2000);
                browser.CheckIfAlertTextEquals("javascript 2 resource loaded!");
                browser.ConfirmAlert();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB);

                // go to first page
                browser.First("a").Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageA + "/16");

                // test first page
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.ElementAt("span", 0).CheckIfTextEquals("3");

                // go to second page
                browser.FindElements("a").Single(l => l.GetText() == "Go to Task List").Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB);

                // try the task list
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);
                browser.Last("tr a").Click();
                browser.Wait();
                browser.Last(".table tr").CheckIfHasClass("completed");

                // test the browse back button
                browser.NavigateBack();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageA + "/16");

                // test first page
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.ElementAt("span", 0).CheckIfTextEquals("3");

                // test the forward button
                browser.NavigateForward();
                browser.Wait();

                // test the redirect inside SPA
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.Last("input[type=button]").Click();
                browser.Wait(2000);
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageB + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PageA + "/15");

                // test the redirect outside SPA
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.FindElements("a").Single(l => l.GetText().Contains("Exit SPA")).Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ComplexSamples_TaskList_ServerRenderedTaskList);
            });
        }

        [TestMethod]
        public void Control_SpaContentPlaceHolder_SpaContentPlaceHolder_PrefixRouteName_EnteredFromPageB()
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageB);
                browser.Wait(2000);

                // verify the URL after redirect to the DefaultRoute
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
                browser.ConfirmAlert();
                browser.Wait(2000);
                browser.CheckIfAlertTextEquals("javascript 2 resource loaded!");
                browser.ConfirmAlert();
                browser.Wait(2000);
                browser.CheckIfAlertTextEquals("javascript 2 resource loaded!");
                browser.ConfirmAlert();
                browser.Wait(2000);
                browser.CheckIfAlertTextEquals("javascript resource loaded!");
                browser.ConfirmAlert();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageB);

                // go to first page
                browser.First("a").Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA + "/16");

                // test first page
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.ElementAt("span", 0).CheckIfTextEquals("3");

                // go to second page
                browser.FindElements("a").Single(l => l.GetText() == "Go to Task List").Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageB);

                // try the task list
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.SendKeys("input[type=text]", "DotVVM rocks!");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(4);
                browser.Last("tr a").Click();
                browser.Wait();
                browser.Last(".table tr").CheckIfHasClass("completed");

                // test the browse back button
                browser.NavigateBack();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA + "/16");

                // test first page
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.Click("input[type=button]");
                browser.Wait();
                browser.ElementAt("span", 0).CheckIfTextEquals("3");

                // test the forward button
                browser.NavigateForward();
                browser.Wait();

                // test the redirect inside SPA
                browser.FindElements(".table tr").ThrowIfDifferentCountThan(3);
                browser.Last("input[type=button]").Click();
                browser.Wait(2000);
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_Default + "#!/" + SamplesRouteUrls.ControlSamples_SpaContentPlaceHolder_PrefixRouteName_PageA + "/15");

                // test the redirect outside SPA
                browser.ElementAt("span", 0).CheckIfTextEquals("0");
                browser.FindElements("a").Single(l => l.GetText().Contains("Exit SPA")).Click();
                browser.Wait();
                browser.CheckUrlEquals(SeleniumTestsConfiguration.BaseUrl + SamplesRouteUrls.ComplexSamples_TaskList_ServerRenderedTaskList);
            });
        }

    }
}
