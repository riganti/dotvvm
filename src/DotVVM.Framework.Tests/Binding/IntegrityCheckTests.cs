using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Testing;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class IntegrityCheckTests
    {
        private const string _integrityHash = "sha256-hwg4gsxgFZhOsEEamdOYGBf13FyQuiTwlAQgxVSNgt4=";
        private const string _jqueryUri = "https://cdnjs.cloudflare.com/ajax/libs/jquery/3.2.1/jquery.min.js";

        private string RenderResource(DotvvmConfiguration configuration, ScriptResource jquery)
        {
            configuration.Freeze();
            var context = new TestDotvvmRequestContext()
            {
                Configuration = configuration,
                ResourceManager = new ResourceManager(configuration.Resources),
                ViewModel = new DotvvmViewModelBase()
            };

            using (var text = new StringWriter())
            {
                var html = new HtmlWriter(text, context);
                jquery.RenderLink(jquery.Location, html, context, "jquery");

                return text.GetStringBuilder().ToString();
            }
        }

        [TestMethod]
        public void IntegrityCheck_NoCheck()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register("jquery", new ScriptResource { Location = new UrlResourceLocation(_jqueryUri), VerifyResourceIntegrity = false });

            var jquery = configuration.Resources.FindResource("jquery") as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsFalse(scriptTag.Contains("integrity"));
            Assert.IsFalse(scriptTag.Contains(_integrityHash));

        }

        [TestMethod]
        public void IntegrityCheck_NoCheck_WithIntegrityHash()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register("jquery", new ScriptResource { Location = new UrlResourceLocation(_jqueryUri), VerifyResourceIntegrity = false, IntegrityHash = _integrityHash });

            var jquery = configuration.Resources.FindResource("jquery") as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsFalse(scriptTag.Contains("integrity"));
            Assert.IsFalse(scriptTag.Contains(_integrityHash));
        }

        [TestMethod]
        public void IntegrityCheck_ShouldFail()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register("jquery", new ScriptResource { Location = new UrlResourceLocation(_jqueryUri), VerifyResourceIntegrity = true, IntegrityHash = "123" });

            var jquery = configuration.Resources.FindResource("jquery") as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsTrue(scriptTag.Contains("integrity"));
            Assert.IsFalse(scriptTag.Contains(_integrityHash));
        }

        [TestMethod]
        public void IntegrityCheck_ShouldSucceed()
        {
            //Arrange
            var configuration = DotvvmTestHelper.CreateConfiguration();

            configuration.Resources.Register("jquery", new ScriptResource { Location = new UrlResourceLocation(_jqueryUri), VerifyResourceIntegrity = true, IntegrityHash = _integrityHash });

            var jquery = configuration.Resources.FindResource("jquery") as ScriptResource;

            //Act
            string scriptTag = RenderResource(configuration, jquery);

            //Assert
            Assert.IsTrue(scriptTag.Contains("integrity"));
            Assert.IsTrue(scriptTag.Contains(_integrityHash));
        }
    }
}
