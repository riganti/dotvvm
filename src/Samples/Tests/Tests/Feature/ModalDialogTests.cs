using System;
using System.Linq;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.Core.Api;
using Riganti.Selenium.DotVVM;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class ModalDialogTests : AppSeleniumTest
    {
        public ModalDialogTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        IElementWrapper OpenDialog(IBrowserWrapper browser, string dialogId)
        {
            var button = browser.Single($"btn-open-{dialogId}", SelectByDataUi);
            AssertUI.IsNotDisplayed(browser.Single(dialogId, SelectByDataUi));
            button.Click();
            AssertUI.HasClass(browser.Single($"btn-open-{dialogId}", SelectByDataUi), "button-active");
            var dialog = browser.Single(dialogId, SelectByDataUi);
            AssertUI.IsDisplayed(dialog);
            return dialog;
        }

        void CheckDialogCloses(IBrowserWrapper browser, string id, Action<IElementWrapper> closeAction)
        {
            var dialog = OpenDialog(browser, id);
            closeAction(dialog);
            AssertUI.IsNotDisplayed(browser.Single(id, SelectByDataUi));
            AssertUI.HasNotClass(browser.Single($"btn-open-{id}", SelectByDataUi), "button-active");
        }

        [Fact]
        public void Feature_ModalDialog_Simple()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ModalDialog_ModalDialog);
                CheckDialogCloses(browser, "simple", dialog => dialog.Single("btn-close", SelectByDataUi).Click());
                CheckDialogCloses(browser, "simple", dialog => {
                    // backdrop click does nothing
                    new Actions(browser.Driver).MoveToLocation(1, 1).Click().Perform();
                    AssertUI.IsDisplayed(dialog);

                    dialog.SendKeys(Keys.Escape);
                    AssertUI.IsNotDisplayed(dialog);
                });

            });
        }

        [Fact]
        public void Feature_ModalDialog_Chained()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ModalDialog_ModalDialog);
                var dialog1 = OpenDialog(browser, "chained1");
                dialog1.Single("btn-next", SelectByDataUi).Click();
                AssertUI.IsNotDisplayed(dialog1);
                var dialog2 = browser.Single("chained2", SelectByDataUi);
                AssertUI.IsDisplayed(dialog2);
                dialog2.Single("btn-close", SelectByDataUi).Click();
                AssertUI.IsNotDisplayed(dialog2);
            });
        }

        [Fact]
        public void Feature_ModalDialog_CloseEvent()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ModalDialog_ModalDialog);

                CheckDialogCloses(browser, "close-event", dialog => dialog.Single("btn-close1", SelectByDataUi).Click());
                AssertUI.InnerTextEquals(browser.Single("close-event-counter", SelectByDataUi), "1");
                CheckDialogCloses(browser, "close-event", dialog => dialog.Single("btn-close2", SelectByDataUi).Click());
                AssertUI.InnerTextEquals(browser.Single("close-event-counter", SelectByDataUi), "2");
                CheckDialogCloses(browser, "close-event", dialog => dialog.Single("btn-close3", SelectByDataUi).Click());
                AssertUI.InnerTextEquals(browser.Single("close-event-counter", SelectByDataUi), "3");
                CheckDialogCloses(browser, "close-event", dialog => dialog.SendKeys(Keys.Escape));
                AssertUI.InnerTextEquals(browser.Single("close-event-counter", SelectByDataUi), "4");

                CheckDialogCloses(browser, "close-event", dialog => {
                    // dialog click
                    new Actions(browser.Driver).MoveToElement(dialog.WebElement, 1, 1).Click().Perform();
                    AssertUI.IsDisplayed(dialog);
                    // backdrop click
                    new Actions(browser.Driver).MoveToLocation(1, 1).Click().Perform();
                });
            });
        }

        [Fact]
        public void Feature_ModalDialog_ModelController()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ModalDialog_ModalDialog);
                CheckDialogCloses(browser, "view-model", dialog => dialog.Single("btn-save", SelectByDataUi).Click());
                CheckDialogCloses(browser, "view-model", dialog => dialog.Single("btn-close", SelectByDataUi).Click());
                CheckDialogCloses(browser, "int", dialog => dialog.Single("btn-close", SelectByDataUi).Click());
                // clearing the numeric input puts null into the nullable integer controller
                CheckDialogCloses(browser, "int", dialog => dialog.Single("editor", SelectByDataUi).Clear());
                CheckDialogCloses(browser, "string", dialog => dialog.Single("btn-close", SelectByDataUi).Click());
            });
        }
    }
}
