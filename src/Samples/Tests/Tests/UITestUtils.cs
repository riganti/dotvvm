using System;
using OpenQA.Selenium;

namespace DotVVM.Samples.Tests;
public static class UITestUtils
{

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
}
