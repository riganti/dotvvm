using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace DotVVM.Framework.Testing.Proxies
{
    public class ValidationSummaryProxy : WebElementProxyBase
    {
        public ValidationSummaryProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public override bool IsVisible()
        {
            var summary = FindElement();
            return summary.FindElements(By.TagName("li")).Count != 0;
        }

        public int GetErrorCount()
        {
            var summary = FindElement();

            return summary.FindElements(By.TagName("li")).Count;
        }

        public List<string> GetErrors()
        {
            var summary = FindElement();

            var summaryItems = summary.FindElements(By.TagName("li"));

            return summaryItems.Select(s => s.Text).ToList();
        }
    }
}
