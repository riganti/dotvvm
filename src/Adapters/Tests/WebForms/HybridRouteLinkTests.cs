using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using CheckTestOutput;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Adapters.WebForms.Tests
{
    [TestClass]
    public class HybridRouteLinkTests
    {
        private static readonly ControlTestHelper cth = new ControlTestHelper(config: config => config.AddWebFormsAdapters());
        OutputChecker check = new OutputChecker("testoutputs");
        
        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            WebFormsRouteTableInit.EnsureInitialized();
        }

        [TestMethod]
        public async Task HybridRouteLink_NoBindings()
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
            );

            var r = await cth.RunPage(typeof(ControlTestViewModel), @"
            <webforms:HybridRouteLink RouteName=NoParams Text='hello 1' />
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index=3 Text='hello 2' />
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index={resource: 15} Text='hello 3' />
            <webforms:HybridRouteLink RouteName=MultipleOptionalParams Text='hello 4' />;
            <webforms:HybridRouteLink RouteName=MultipleOptionalParams Param-Tag=aaa Text='hello 5' />;
            <webforms:HybridRouteLink RouteName=MultipleOptionalParams Param-SubTag=bbb Text='hello 6' />
            <webforms:HybridRouteLink RouteName=MultipleOptionalParams Param-Tag=aaa Param-SubTag=bbb Text='hello 6' />");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task HybridRouteLink_ValueBinding()
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
            );

            var r = await cth.RunPage(typeof(ControlTestViewModel), @"
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index={value: Value} Text='hello 3' />
            <dot:Repeater DataSource={value: Items}>
                <webforms:HybridRouteLink RouteName=SingleParam Param-Index={value: Id} Text={value: Name} />
            </dot:Repeater>");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }

        [TestMethod]
        public async Task HybridRouteLink_SuffixAndQueryString()
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
            );

            var r = await cth.RunPage(typeof(ControlTestViewModel), @"
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index={value: Value} UrlSuffix='?hello=1' Text='hello 1' />
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index={value: Value} UrlSuffix={value: '?hello=' + Value} Text='hello 2' />
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index={value: Value} UrlSuffix={value: '?hello=' + Value} Query-Test=1 Text='hello 3' />
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index={value: Value} UrlSuffix={value: '?hello=' + Value} Query-Test={value: Value * 2} Text='hello 4' />
            <webforms:HybridRouteLink RouteName=SingleParam Param-Index={value: Value} Query-Test1={value: Value + Items.Count} Query-Id=abc Text='hello 5' />");

            check.CheckString(r.FormattedHtml, fileExtension: "html");
        }
    }

    class ControlTestViewModel
    {
        public int Value { get; set; } = 15;

        public List<ControlTestChildViewModel> Items { get; set; } = new()
        {
            new ControlTestChildViewModel() { Id = 1, Name = "one" },
            new ControlTestChildViewModel() { Id = 2, Name = "two" },
            new ControlTestChildViewModel() { Id = 3, Name = "three" }
        };
    }

    class ControlTestChildViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
