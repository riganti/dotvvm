using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Security;
using DotVVM.Testing.SeleniumGenerator.Tests.Helpers;
using DotVVM.Testing.SeleniumGenerator.Tests.Visitors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;

namespace DotVVM.Testing.SeleniumGenerator.Tests
{
    [TestClass]
    public class SampleApp1Tests
    {
        private readonly string solutionDirectory;
        private readonly string webAppDirectory;
        private readonly string proxiesCsProjPath;

        public TestContext TestContext { get; set; }

        public SampleApp1Tests()
        {
            solutionDirectory = TestEnvironmentHelper.FindSolutionDirectory();
            webAppDirectory = Path.Combine(solutionDirectory, "Samples\\SampleApp1");
            proxiesCsProjPath = Path.Combine(solutionDirectory, "DotVVM.Framework.Testing.SeleniumHelpers\\DotVVM.Framework.Testing.SeleniumHelpers.csproj");
        }

        // todo: otestovat DetermineName metodu

        [TestMethod]
        public async Task SimplePage_CheckGeneratedProperties()
        {
            using (var workspace = new WebApplicationHost(TestContext, webAppDirectory))
            {
                workspace.ProcessMarkupFile("Views/SimplePage/Page.dothtml");

                // compile project
                workspace.FixReferencedProjectPath(proxiesCsProjPath);
                var compilation = await workspace.CompileAsync();

                // verify the class
                var pageObject = compilation.AssertPageObject("SampleApp1.Tests.PageObjects.SimplePage", "PagePageObject");
                pageObject.AssertPublicProperty(typeof(RadioButtonProxy), "Person");
                pageObject.AssertPublicProperty(typeof(RadioButtonProxy), "Company");
                pageObject.AssertPublicProperty(typeof(TextBoxProxy), "Name_FirstName");
                pageObject.AssertPublicProperty(typeof(TextBoxProxy), "Name_LastName");
                pageObject.AssertPublicProperty(typeof(ButtonProxy), "Click");
                pageObject.AssertPublicProperty(typeof(TextBoxProxy), "Address");
                pageObject.AssertPublicProperty(typeof(ComboBoxProxy), "CountryCode");
                pageObject.AssertPublicProperty(typeof(CheckBoxProxy), "IsEuVatPayer");
                pageObject.AssertPublicProperty(typeof(ButtonProxy), "CreateCompany");
                pageObject.AssertPublicProperty(typeof(LinkButtonProxy), "ResetForm");
                pageObject.AssertPublicProperty(typeof(LiteralProxy), "StatusMessage");
                pageObject.AssertPublicProperty(typeof(ValidationSummaryProxy), "ValidationSummary");
            }
        }

        [TestMethod]
        public async Task TestPage_CheckSeparateRepeaterPageObject()
        {
            using (var workspace = new WebApplicationHost(TestContext, webAppDirectory))
            {
                workspace.ProcessMarkupFile("Views/SimplePage/TestPage.dothtml");

                // compile project
                workspace.FixReferencedProjectPath(proxiesCsProjPath);
                var compilation = await workspace.CompileAsync();

                // verify the class
                compilation.AssertPageObject("SampleApp1.Tests.PageObjects.SimplePage", "TestPagePageObject");
                compilation.AssertPageObject("SampleApp1.Tests.PageObjects.SimplePage.TestPagePageObject", "UsersRepeaterPageObject");
            }
        }

        // TODO: Remake DotVVM.CommandLine.Common and then uncomment this test.
        //[TestMethod]
        //public async Task SimplePage_CheckGeneratedUiNames()
        //{
        //    using (var workspace = new WebApplicationHost(TestContext, webAppDirectory))
        //    {
        //        var processedFileContent = workspace.ProcessMarkupFile("Views/SimplePage/Page.dothtml");

        //        // compile project
        //        workspace.FixReferencedProjectPath(proxiesCsProjPath);
        //        var compilation = await workspace.CompileAsync();

        //        // verify the class
        //        compilation.AssertPageObject("SampleApp1.Tests.PageObjects.SimplePage", "PagePageObject");

        //        // get dotvvm config
        //        var config = DotvvmProject.GetConfiguration(
        //            Assembly.LoadFile(Path.Combine(Path.GetFullPath(webAppDirectory), "bin\\debug\\netcoreapp2.0\\SampleApp1.dll")),
        //            webAppDirectory,
        //            services => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());

        //        // get abstract tree
        //        var tree = ResolveControlTree(processedFileContent, config);

        //        // get and check results
        //        var visitor = new UiNamesTestingVisitor();
        //        GetControlsWithSelectors(tree, visitor);
        //        var results = visitor.GetResult();

        //        Assert.AreEqual(results.Count, 12);
        //        AssertControlSelector((nameof(RadioButton), "Person"), results[0]);
        //        AssertControlSelector((nameof(RadioButton), "Company"), results[1]);
        //        AssertControlSelector((nameof(TextBox), "Name_FirstName"), results[2]);
        //        AssertControlSelector((nameof(TextBox), "Name_LastName"), results[3]);
        //        AssertControlSelector((nameof(Button), "Click"), results[4]);
        //        AssertControlSelector((nameof(TextBox), "Address"), results[5]);
        //        AssertControlSelector((nameof(CheckBox), "IsEuVatPayer"), results[6]);
        //        AssertControlSelector((nameof(ComboBox), "CountryCode"), results[7]);
        //        AssertControlSelector((nameof(Button), "CreateCompany"), results[8]);
        //        AssertControlSelector((nameof(LinkButton), "ResetForm"), results[9]);
        //        AssertControlSelector((nameof(Literal), "StatusMessage"), results[10]);
        //        AssertControlSelector((nameof(ValidationSummary), "ValidationSummary"), results[11]);
        //    }
        //}

        // TODO: Remake DotVVM.CommandLine.Common and then uncomment this test.
        //[TestMethod]
        //public async Task SimplePage_CheckDataContextDependingSelectors()
        //{
        //    using (var workspace = new WebApplicationHost(TestContext, webAppDirectory))
        //    {
        //        var processedFileContent = workspace.ProcessMarkupFile("Views/SimplePage/Page.dothtml");

        //        // compile project
        //        workspace.FixReferencedProjectPath(proxiesCsProjPath);
        //        var compilation = await workspace.CompileAsync();

        //        // verify the class
        //        compilation.AssertPageObject("SampleApp1.Tests.PageObjects.SimplePage", "PagePageObject");

        //        // get dotvvm config
        //        var config = DotvvmProject.GetConfiguration(
        //            Assembly.LoadFile(Path.Combine(Path.GetFullPath(webAppDirectory), "bin\\debug\\netcoreapp2.0\\SampleApp1.dll")),
        //            webAppDirectory,
        //            services => services.TryAddSingleton<IViewModelProtector, FakeViewModelProtector>());

        //        // get abstract tree
        //        var tree = ResolveControlTree(processedFileContent, config);

        //        // traverse tree and get all controls with selectors AND dataContexts
        //        var visitor = new DataContextSelectorsTestingVisitor();
        //        GetControlsWithSelectors(tree, visitor);
        //        var results = visitor.GetResult();

        //        // check number of properties with dataContext prefix
        //        Assert.AreEqual(results.Count, 2);

        //        // check correctness of dataContext prefixes
        //        foreach (var result in results)
        //        {
        //            var split = result.Selector.Substring(0, result.Selector.LastIndexOf('_'));
        //            Assert.AreEqual(split, result.DataContext);
        //        }
        //    }
        //}

        private static void GetControlsWithSelectors(IAbstractTreeRoot tree, IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitView((ResolvedTreeRoot) tree);
        }

        private void AssertControlSelector(
            (string expectedControlName, string expectedSelectorName) expected, 
            (string controlName, string selectorName) result)
        {
            Assert.AreEqual(expected.expectedControlName, result.controlName);
            Assert.AreEqual(expected.expectedSelectorName,result.selectorName);
        }

        private IAbstractTreeRoot ResolveControlTree(string fileContent, DotvvmConfiguration dotvvmConfiguration)
        {
            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(fileContent);

            var parser = new DothtmlParser();
            var rootNode = parser.Parse(tokenizer.Tokens);

            var treeResolver = dotvvmConfiguration.ServiceProvider.GetService<IControlTreeResolver>();
            return treeResolver.ResolveTree(rootNode, "Views/SimplePage/Page.dothtml");
        }
    }
}
