using DotVVM.Samples.Tests.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;

namespace DotVVM.Samples.Tests
{
    public static class HelpersExtensions
    {
        public static string CheckTextValue(this SeleniumElementHelper element, string expectedValue, bool textCanBeNull = false)
        {
            var tagName = element.GetTagName().ToLower();

            string buttonValue;
            if (tagName == "input" || tagName == "textarea")
            {
                buttonValue = element.GetAttribute("value");
            }
            else
            {
                buttonValue = element.GetText();
            }

            //check value
            if (expectedValue != null)
            {
                StringAssert.IsNotNullOrWhiteSpace(buttonValue, $"Element has wrong text value. Expected value: '{expectedValue}', Provided value: '{buttonValue}'");
            }
            else if (!textCanBeNull)
            {
                StringAssert.IsNotNullOrWhiteSpace(buttonValue, $"Inner text property is null.");
            }
            return buttonValue;
        }

        public static string CheckTagName(this SeleniumElementHelper element, string expectedTagName)
        {
            var tagName = element.GetTagName().ToLower();
            Assert.AreEqual(tagName, expectedTagName, $"Element has wrong tagName. Expected value: '{expectedTagName}', Provided value: '{element.GetTagName()}'");
            return tagName;
        }

        public static IWebElement FirstByCssSelector(this IWebDriver driver, string selector)
        {
            var elements = driver.FindElements(By.CssSelector(selector));
            if (elements.Count == 0)
            {
                throw new NoSuchElementException($"Element not found. Selector: {selector}");
            }
            return elements[0];
        }

        public static IWebElement SingleByCssSelector(this IWebDriver driver, string selector)
        {
            var elements = driver.FindElements(By.CssSelector(selector));
            if (elements.Count > 1)
            {
                throw new MoreElementsInSequenceException($"Sequence containse more then one element. Selector: '{selector}'");
            }
            if (elements.Count == 0)
            {
                throw new NoSuchElementException($"Sequence containse more then one element. Selector: '{selector}'");
            }
            return elements[0];
        }
    }
}