using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.Base;
using DotVVM.Testing.Abstractions;
using OpenQA.Selenium;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;

namespace DotVVM.Samples.Tests.Feature
{
    public class HotReloadTests : AppSeleniumTest
    {
        public HotReloadTests(Xunit.Abstractions.ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Feature_HotReload_ViewChanges()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_HotReload_ViewChanges);

                BackupDothtmlFileAndRun(browser, a => {

                    browser.FindElements("input[type=text]").ThrowIfDifferentCountThan(1);
                    browser.ElementAt("input[type=text]", 0).SendKeys("hello").SendKeys(Keys.Tab);
                    browser.Wait(1000);

                    var updated = a.original.Replace("###", "<dot:TextBox Text='{value: Value}' />");
                    a.writeContents(updated);

                    browser.FindElements("input[type=text]").ThrowIfDifferentCountThan(2);
                    AssertUI.Value(browser.ElementAt("input[type=text]", 0), "hello");
                    AssertUI.Value(browser.ElementAt("input[type=text]", 1), "hello");
                });
            });
        }

        [Fact]
        public void Feature_HotReload_ErrorPage()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_HotReload_ViewChanges);

                BackupDothtmlFileAndRun(browser, a => {

                    AssertUI.TextEquals(browser.First("h1"), "Hot Reload test");

                    var updated = a.original.Replace("###", "<dot:TextBox Text='{value: NonExistentValue}' />");
                    a.writeContents(updated);

                    browser.Wait(1000);
                    AssertUI.TextEquals(browser.First("h1"), "Server Error, HTTP 500: Unhandled exception occurred");

                    updated = a.original.Replace("<dot:TextBox Text='{value: NonExistentValue}' />", "<dot:TextBox Text='{value: Value}' />");
                    a.writeContents(updated);

                    browser.Wait(1000);
                    AssertUI.TextEquals(browser.First("h1"), "Hot Reload test");
                });
            });
        }

        private void BackupDothtmlFileAndRun(IBrowserWrapper browser, Action<(string original, Action<string> writeContents)> action)
        {
            // backup dothtml file contents
            var fileName = browser.Single(".dothtml-filename").GetInnerText();
            var backupFileName = fileName + ".bak";
            var original = File.ReadAllText(fileName, Encoding.UTF8);
            File.Copy(fileName, backupFileName, overwrite: true);

            try
            {
                action((original, newContents => File.WriteAllText(fileName, newContents, Encoding.UTF8)));
            }
            finally
            {
                // restore dothtml file contents
                File.Move(backupFileName, fileName, overwrite: true);
                File.Delete(backupFileName);
            }
        }
    }
}
