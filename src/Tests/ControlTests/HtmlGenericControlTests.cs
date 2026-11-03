using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Testing;
using System.Security.Claims;
using System.Collections;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;
using AngleSharp.Html.Parser;

namespace DotVVM.Framework.Tests.ControlTests
{
    [TestClass]
    public class HtmlGenericControlTests
    {
        static readonly ControlTestHelper cth = new ControlTestHelper(config: config => {
        });
        static readonly BindingCompilationService bindingService = cth.GetService<BindingCompilationService>();
        static readonly IDotvvmRequestContext defaultContext = DotvvmTestHelper.CreateContext(cth.Configuration);
        static readonly DataContextStack defaultDataContext = DataContextStack.Create(typeof(BasicTestViewModel), extensionParameters: new [] { new JsExtensionParameter("p1", false) });
        readonly OutputChecker check = new OutputChecker("testoutputs");

        string RenderToString(DotvvmControl control, object viewModel = null, IDotvvmRequestContext context = null)
        {
            context ??= defaultContext;
            viewModel ??= new BasicTestViewModel();

            control.SetValue(Internal.RequestContextProperty, context);
            control.DataContext = viewModel;
            using  (var sw = new System.IO.StringWriter())
            {
                var html = new HtmlWriter(sw, context);
                control.Render(html, context);
                return sw.ToString();
            }
        }

        // Test that all HtmlGenericControl properties are bindable and the value binding is evaluated both client-side and server-side, regardless of the RenderMode.
        // Errors in value bindings are suppressed, to allow bindings which can't be evaluated server-side
        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void JsInvoke_Attribute(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .SetAttribute("data-test", bindingService.Cache.CreateValueBinding<string>("_js.Invoke<string>('testMethod')", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual("""<div data-bind='attr: { "data-test": dotvvm.viewModules.call("p1", "testMethod", [], false) }'></div>""", str);
        }

        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void JsInvoke_Visible(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .SetProperty(c => c.Visible, bindingService.Cache.CreateValueBinding<bool>("_js.Invoke<bool>('testMethod')", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual("""<div data-bind='visible: dotvvm.viewModules.call("p1", "testMethod", [], false)'></div>""", str);
        }
        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void JsInvoke_Class(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .AddCssClass("test-class", bindingService.Cache.CreateValueBinding<bool>("_js.Invoke<bool>('testMethod')", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual("""<div data-bind='css: { "test-class": dotvvm.viewModules.call("p1", "testMethod", [], false) }'></div>""", str);
        }
        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void JsInvoke_Style(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .AddCssStyle("width", bindingService.Cache.CreateValueBinding<int>("_js.Invoke<int>('testMethod')", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual("""<div data-bind='style: { width: dotvvm.viewModules.call("p1", "testMethod", [], false) }'></div>""", str);
        }

        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void ValueBinding_Attribute(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .SetAttribute("data-test", bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual($$"""<div data-test=some-string data-bind='attr: { "data-test": String }'></div>""", str);
        }

        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void ValueBinding_Visible(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .SetProperty(c => c.Visible, bindingService.Cache.CreateValueBinding<bool>("Integer == 99", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual("""<div style=display:none data-bind="visible: Integer() == 99"></div>""", str);
        }

        [TestMethod]
        public void VisibleFalse_PreservesCssStyles()
        {
            var control = new HtmlGenericControl("div") { Visible = false };
            control.CssStyles.Add("width", "123px");

            Assert.AreEqual("""<div style="width:123px;display:none"></div>""", RenderToString(control));
        }

        [TestMethod]
        public void LiteralAsInnerText_WithoutChildSettings()
        {
            var literal = new Literal(bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext));
            var control = new HtmlGenericControl("div") { Children = { literal } };

            Assert.AreEqual("""<div data-bind="text: String"></div>""", RenderToString(control));
        }

        [TestMethod]
        public void LiteralAsInnerText_PreservesIncludeInPageConstant()
        {
            var literal = new Literal(bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext)) {
                IncludeInPage = false
            };
            var control = new HtmlGenericControl("div") { Children = { literal } };

            Assert.AreEqual("<div></div>", RenderToString(control));
        }

        [TestMethod]
        public void LiteralAsInnerText_PreservesIncludeInPageResourceBinding()
        {
            var literal = new Literal(bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext));
            literal.SetBinding(DotvvmControl.IncludeInPageProperty,
                bindingService.Cache.CreateResourceBinding<bool>("Boolean", defaultDataContext));
            var control = new HtmlGenericControl("div") { Children = { literal } };

            Assert.AreEqual("<div></div>", RenderToString(control));
        }

        [TestMethod]
        public void LiteralAsInnerText_PreservesIncludeInPageValueBinding()
        {
            var literal = new Literal(bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext));
            literal.SetBinding(DotvvmControl.IncludeInPageProperty,
                bindingService.Cache.CreateValueBinding<bool>("!Boolean", defaultDataContext));
            var control = new HtmlGenericControl("div") { Children = { literal } };

            Assert.AreEqual("<div><!-- ko if: !Boolean() --><!-- ko text: String --><!-- /ko --><!-- /ko --></div>", RenderToString(control));
        }

        [TestMethod]
        public void LiteralAsInnerText_PreservesDataContextBinding()
        {
            var childDataContext = DataContextStack.Create(typeof(string), defaultDataContext);
            var literal = new Literal(bindingService.Cache.CreateValueBinding<string>("_this", childDataContext));
            literal.SetBinding(DotvvmBindableObject.DataContextProperty,
                bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext));
            var control = new HtmlGenericControl("div") { Children = { literal } };
            control.SetValue(RenderSettings.ModeProperty, RenderMode.Server);

            Assert.AreEqual("<div><!-- ko with: String --><!-- ko text: $rawData -->some-string<!-- /ko --><!-- /ko --></div>", RenderToString(control));
        }

        [TestMethod]
        public void LiteralAsInnerText_PreservesChildRenderMode()
        {
            var literal = new Literal(bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext));
            literal.SetValue(RenderSettings.ModeProperty, RenderMode.Server);
            var control = new HtmlGenericControl("div") { Children = { literal } };

            Assert.AreEqual("<div><!-- ko text: String -->some-string<!-- /ko --></div>", RenderToString(control));
        }

        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void ValueBinding_Class(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .AddCssClass("test-class", bindingService.Cache.CreateValueBinding<bool>("Integer > 0", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual("""<div class=test-class data-bind='css: { "test-class": Integer() &gt; 0 }'></div>""", str);
        }

        [DataTestMethod]
        [DataRow(RenderMode.Client)]
        [DataRow(RenderMode.Server)]
        public void ValueBinding_Style(RenderMode renderMode)
        {
            var control = new HtmlGenericControl("div")
                .SetProperty(RenderSettings.ModeProperty, renderMode)
                .AddCssStyle("width", bindingService.Cache.CreateValueBinding<int>("Integer", defaultDataContext));

            var str = RenderToString(control);
            Assert.AreEqual("""<div style=width:123 data-bind="style: { width: Integer }"></div>""", str);
        }

        [DataTestMethod]
        [DataRow("binding;middle;binding", "String()+\";middle;\"+String()")]
        [DataRow("binding;middle;tail;binding", "String()+\";middle;\"+\"tail;\"+String()")]
        [DataRow("binding;middle;tail", "String()+\";middle;\"+\"tail\"")]
        [DataRow("prefix;binding;middle;binding", "\"prefix;\"+String()+\";middle;\"+String()")]
        [DataRow("binding;binding;middle;binding", "String()+\";\"+String()+\";middle;\"+String()")]
        [DataRow("prefix;binding", "\"prefix;\"+String()")]
        public void AttributeList_MixedBindingsAndConstants_SingleSeparators(string values, string expectedExpression)
        {
            var binding = bindingService.Cache.CreateValueBinding<string>("String", defaultDataContext);
            var items = values.Split(';');
            HtmlGenericControl.AttributeList list = null;
            for (var i = items.Length - 1; i >= 0; i--)
            {
                list = new HtmlGenericControl.AttributeList(items[i] == "binding" ? binding : items[i], list);
            }

            foreach (var (attributeName, separator) in new[] { ("class", " "), ("data-test", ";") })
            {
                var control = new HtmlGenericControl("div").SetAttribute(attributeName, list);
                var element = new HtmlParser().ParseDocument(RenderToString(control)).Body.FirstElementChild;
                var expression = expectedExpression.Replace(";", separator);
                var expectedDataBind = attributeName == "class"
                    ? "class: " + expression
                    : "attr: { \"data-test\": " + expression + " }";

                Assert.AreEqual(values.Replace("binding", "some-string").Replace(";", separator), element.GetAttribute(attributeName));
                Assert.AreEqual(expectedDataBind, element.GetAttribute("data-bind"));
            }
        }

        [TestMethod]
        public async Task ClassAttribute_HtmlCapabilityAndAttributesCollection_StaticValues()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), """
                <div Class=from-capability class=from-attributes />
                """);

            Assert.AreEqual("""
                            
                            
                            <head></head>
                            <body>
                            <div class="from-capability from-attributes"></div>
                            
                            
                            </body>
                            """.Replace("\r\n", "\n"), r.OutputString.Replace("\r\n", "\n"));
        }

        [TestMethod]
        public async Task ClassAttribute_HtmlCapabilityAndAttributesCollection_ValueBinding()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), """
                <div Class=static-class class={value: String} />
                """);

            Assert.AreEqual("""
                            
                            
                            <head></head>
                            <body>
                            <div class="static-class some-string" data-bind='class: "static-class " + (String() ?? "")'></div>
                            
                            
                            </body>
                            """.Replace("\r\n", "\n"), r.OutputString.Replace("\r\n", "\n"));
        }

        [TestMethod]
        public async Task ClassAttribute_HtmlCapabilityAndAttributesCollection_BothValueBindings()
        {
            var r = await cth.RunPage(typeof(BasicTestViewModel), """
                <div Class={value: String} class={value: String} />
                """);

            Assert.AreEqual("""
                            
                            
                            <head></head>
                            <body>
                            <div class="some-string some-string" data-bind='class: (String() ?? "") + " " + (String() ?? "")'></div>
                            
                            
                            </body>
                            """.Replace("\r\n", "\n"), r.OutputString.Replace("\r\n", "\n"));
        }

        public class BasicTestViewModel: DotvvmViewModelBase
        {
            public int Integer { get; set; } = 123;
            public bool Boolean { get; set; } = false;
            public string String { get; set; } = "some-string";
        }
    }
}
