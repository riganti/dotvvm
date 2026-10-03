using System;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Riganti.Selenium.DotVVM;

namespace DotVVM.Samples.Tests;
public static class UITestUtils
{
    public static void ClickAndWaitForPageLoad(this IElementWrapper element)
    {
        var browser = element.BrowserWrapper;
        var oldDocument = browser.First("html").WebElement;
        element.Click();

        // The URL and rendered content can stay the same after an authentication redirect.
        browser.WaitFor(() => {
            try
            {
                oldDocument.GetAttribute("id");
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        }, timeout: 10000, failureMessage: "The page did not reload after clicking.", ignoreCertainException: false);
        browser.WaitUntilDotvvmInited();
    }

    public static T StaleElementRetry<T>(Func<T> action, int attempts = 5)
    {
        if (attempts <= 0)
            return action();

        try
        {
            return action();
        }
        catch (StaleElementReferenceException)
        {
            return StaleElementRetry<T>(action, attempts - 1);
        }
    }
    public static void StaleElementRetry(Action action, int attempts = 5) =>
        StaleElementRetry(() => { action(); return 0; }, attempts);


    public static void WaitForIgnoringStaleElements(Action action, WaitForOptions options = null)
    {
        WaitForExecutor.WaitFor(() => StaleElementRetry(action), options);
    }
}
