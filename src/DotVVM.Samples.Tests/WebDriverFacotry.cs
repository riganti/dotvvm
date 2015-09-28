using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.Tests
{
    public class WebDriverFacotry
    {
        // ReSharper disable once InconsistentNaming
        public bool IEWebDriverEnabled { get; set; }

        public bool ChromeWebDriverEnabled { get; set; }
        public bool FirefoxWebDriverEnabled { get; set; }

        //DO NOT CHANGE NAME OF THIS FILE
        public const string TestSettingsPath = "TestSettings.config";

        public const string DefaultSettingConfigPath = "TestDefaultValues.config";

        public string ExecutingDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public WebDriverFacotry()
        {
            try
            {
                if (!File.Exists(Path.Combine(ExecutingDirectory, TestSettingsPath)))
                {
                    File.Copy(Path.Combine(ExecutingDirectory, DefaultSettingConfigPath), Path.Combine(ExecutingDirectory, TestSettingsPath));
                }
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                // ignored
            }

            ChromeWebDriverEnabled = GetBrowserConfigurationValue("StartChromeDriver");
            IEWebDriverEnabled = GetBrowserConfigurationValue("StartIEDriver");
            FirefoxWebDriverEnabled = GetBrowserConfigurationValue("StartFirefoxDriver");
        }

        private bool GetBrowserConfigurationValue(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                bool tmp;
                bool.TryParse(ConfigurationManager.AppSettings[key], out tmp);
                return tmp;
            }
            return false;
        }

        public List<Func<IWebDriver>> GetDriverFactories()
        {
            var drivers = new List<Func<IWebDriver>>();
            if (IEWebDriverEnabled)
            {
                drivers.Add(() => new InternetExplorerDriver());
            }

            if (ChromeWebDriverEnabled)
            {
                drivers.Add(() =>
                {
                    var options = new ChromeOptions();
                    options.AddArgument("test-type");
                    return new ChromeDriver(options);
                });
            }

            if (FirefoxWebDriverEnabled)
            {
                drivers.Add(() => new FirefoxDriver());
            }

            return drivers;
        }
    }
}