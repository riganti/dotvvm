using Dotvvm.Samples.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Utils.Testing.Selenium.Core;
using System;

namespace DotVVM.Samples.Tests.Control
{
    [TestClass]
    public class IncludeInPagePropertyTests : SeleniumTest
    {
        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_GridView()
        {
            CheckIncludeInPage(browser =>
            {
                var gridView = browser.Single("gridView", this.SelectByDataUi);
                gridView.CheckIfIsDisplayed();
                gridView.CheckIfContainsElement("thead");
                gridView.CheckIfContainsElement("tbody");
            }, browser =>
            {
                Assert.AreEqual(0, browser.FindElements("gridView", this.SelectByDataUi).Count);
            });
        }

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_GridViewEmptyDataTemplate()
        {
            const string gridViewDataUi = "gridView-emptyDataTemplate";
            const string messageDataUi = "emptyDataTemplate";

            CheckIncludeInPage(browser =>
            {
                browser.CheckIfIsNotDisplayed(gridViewDataUi, this.SelectByDataUi);
                var message = browser.Single(messageDataUi, this.SelectByDataUi);
                message.CheckIfIsDisplayed();
                message.CheckIfTextEquals("There are no Customers to display");
            }, browser =>
            {
                Assert.AreEqual(0, browser.FindElements(gridViewDataUi).Count);
                Assert.AreEqual(0, browser.FindElements(messageDataUi).Count);
            });
        }

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_Literal()
        {
            CheckIncludeInPage(browser =>
            {
                var literal = browser.Single("literal", this.SelectByDataUi);
                literal.CheckIfIsDisplayed();
                literal.CheckIfTextEquals("Test 1");
            }, browser =>
            {
                Assert.AreEqual(0, browser.FindElements("literal", this.SelectByDataUi).Count);
            });
        }

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_LiteralsInRepeater()
        {
            CheckIncludeInPage(browser =>
            {
                var literals = browser.FindElements("literal-repeater", this.SelectByDataUi);
                Assert.AreEqual(3, literals.Count);
                foreach (var literal in literals)
                {
                    literal.CheckIfIsDisplayed();
                }
            }, browser =>
            {
                Assert.AreEqual(0, browser.FindElements("literal-repeater", this.SelectByDataUi).Count);
            });
        }

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_RepeaterFirst() => CheckRepeater("repeater-first", 2);

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_RepeaterSecond() => CheckRepeater("repeater-second", 3);

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBox() => CheckTextBox("textbox", "Default text");

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBoxWithDataContext() => CheckTextBox("textbox-dataContext", "John Smith");

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBoxWithVisible() => CheckTextBox("textbox-visible", "Default text", true);

        [TestMethod]
        public void Control_IncludeInPageProperty_IncludeInPage_TextBoxWithVisibleAndDataContext() => CheckTextBox("textbox-visible-dataContext", "John Smith", true);

        private void CheckIncludeInPage(Action<BrowserWrapper> beforeSwitch, Action<BrowserWrapper> afterSwitch)
        {
            RunInAllBrowsers(browser =>
            {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_IncludeInPageProperty_IncludeInPage);
                browser.Wait();
                beforeSwitch(browser);
                browser.Single("switch-includeInPage", this.SelectByDataUi).Click().Wait();
                afterSwitch(browser);
            });
        }

        private void CheckRepeater(string dataUi, int childrenCount)
        {
            CheckIncludeInPage(browser =>
            {
                var repeater = browser.First(dataUi, this.SelectByDataUi);
                repeater.CheckIfIsDisplayed();
                Assert.AreEqual(childrenCount, repeater.Children.Count);
            }, browser =>
            {
                Assert.AreEqual(0, browser.FindElements(dataUi, this.SelectByDataUi).Count);
            });
        }

        private void CheckTextBox(string dataUi, string text, bool checkVisible = false)
        {
            CheckIncludeInPage(browser =>
            {
                var textBox = browser.Single(dataUi, this.SelectByDataUi);
                textBox.CheckIfTextEquals(text);
                textBox.CheckIfIsDisplayed();
                if (checkVisible)
                {
                    var switchVisible = browser.Single("switch-visible", this.SelectByDataUi);
                    switchVisible.Click().Wait();
                    textBox.CheckIfIsNotDisplayed();
                    switchVisible.Click().Wait();
                    textBox.CheckIfIsDisplayed();
                }
            }, browser =>
            {
                Assert.AreEqual(0, browser.FindElements(dataUi, this.SelectByDataUi).Count);
            });
        }
    }
}