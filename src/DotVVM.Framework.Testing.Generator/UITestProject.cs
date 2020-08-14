using System.IO;
using System.Threading.Tasks;

namespace DotVVM.Framework.Testing.Generator
{
    public static class UITestProject
    {
        public const string SeleniumTestBaseName = "AppSeleniumTest";

        public static async Task<FileInfo?> GenerateStub(
            string webProjectPath,
            string name,
            DirectoryInfo directory,
            string @namespace)
        {
            // TODO: Add 'targetFramework' to DotvvmMetadata
            var targetFramework = "netcoreapp3.1";
            // TODO: Figure out the version of DotVVM.Framework.Testing properly.
            // TODO: Add an option to override the PackageReference with a ProjectRefererence.
            var frameworkTestingVersion = "0.0.0";
            var projectFileText = GetProjectFile(targetFramework, frameworkTestingVersion, webProjectPath);
            var projectFile = new FileInfo(Path.Combine(directory.FullName, $"{name}.csproj"));
            await File.WriteAllTextAsync(projectFile.FullName, projectFileText);

            var seleniumBaseText = GetSeleniumTestBase(@namespace);
            var seleniumBaseFile = new FileInfo(Path.Combine(directory.FullName, $"{SeleniumTestBaseName}.cs"));
            await File.WriteAllTextAsync(seleniumBaseFile.FullName, seleniumBaseText);
        }

        public static string GetProjectFile(string targetFramework, string frameworkTestingVersion, string projectPath)
        {
            return
$@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>{targetFramework}</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""15.9.0"" />
    <PackageReference Include=""Riganti.Selenium.AssertApi"" Version=""2.0.5-preview04-final"" />
    <PackageReference Include=""Riganti.Selenium.DotVVM"" Version=""2.0.5-preview04-final"" />
    <PackageReference Include=""Riganti.Selenium.xUnitIntegration"" Version=""2.0.5-preview04-final"" />
    <PackageReference Include=""xunit"" Version=""2.4.1"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.4.1""/>
    <PackageReference Include=""DotVVM.Framework.Testing"" Version=""{frameworkTestingVersion}"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""{projectPath}"" />
  </ItemGroup>
</Project>
";
        }

        public static string GetSeleniumTestBase(string @namespace)
        {
            return
$@"
using System;
using System.Runtime.CompilerServices;
using Riganti.Selenium.AssertApi;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit.Sdk;

namespace {@namespace}
{{
    public abstract class {SeleniumTestBaseName} : SeleniumTest
    {{
        protected {SeleniumTestBaseName}() : base(new TestOutputHelper())
        {{
        }}
        protected void RunInAllBrowsers<T>(
            Action<IBrowserWrapper, T> action,
            [CallerMemberName] string callerMemberName = """",
            [CallerFilePath] string callerFilePath = """",
            [CallerLineNumber] int callerLineNumber = 0)
            where T : SeleniumHelperBase
        {{
            AssertApiSeleniumTestExecutorExtensions.RunInAllBrowsers(this,
                browser =>
                {{
                    var internalBrowser = browser._GetInternalWebDriver();
                    var pageObject = Activator.CreateInstance(typeof(T), internalBrowser, null, null);
                    browser.NavigateToUrl();
                    action(browser, (T)pageObject);
                }},
                callerMemberName,
                callerFilePath,
                callerLineNumber);
        }}
    }}

    public static class Extensions
    {{
        public static T InitRootPageObject<T>(this IBrowserWrapper wrapper) where T : SeleniumHelperBase
        {{
            var internalBrowser = wrapper._GetInternalWebDriver();
            var pageObject = (T)Activator.CreateInstance(typeof(T), internalBrowser, null, null);
            return pageObject;
        }}
    }}
}}
";
        }
    }
}
