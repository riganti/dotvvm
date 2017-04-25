using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ControlRenderedHtmlTests
    {
        private static TestDotvvmRequestContext CreateContext(object viewModel, DotvvmConfiguration configuration = null)
        {
            configuration = configuration ?? DotvvmConfiguration.CreateDefault();
            return new TestDotvvmRequestContext() {
                Configuration = configuration,
                ResourceManager = new ResourceManagement.ResourceManager(configuration),
                ViewModel = viewModel
            };
        }

        private static string InvokeLifecycleAndRender(DotvvmControl control, TestDotvvmRequestContext context)
        {
            var view = context.View = new DotvvmView();
            view.Children.Add(control);
            view.DataContext = context.ViewModel;
            view.SetValue(Internal.RequestContextProperty, context);

            DotvvmControlCollection.InvokePageLifeCycleEventRecursive(view, LifeCycleEventType.PreRenderComplete);
            using (var text = new StringWriter())
            {
                var html = new HtmlWriter(text, context);
                view.Render(html, context);
                return text.ToString();
            }
        }

        [TestMethod]
        public void GridViewTextColumn_RenderedHtmlTest_ServerRendering()
        {
            var gridView = new GridView() {
                Columns = new List<GridViewColumn>
                {
                    new GridViewTextColumn() { HeaderCssClass = "lol", HeaderText="Header Text", ValueBinding = new ValueBindingExpression(h => h[0], "$this") }
                },
                DataSource = new ValueBindingExpression(h => h[0], "$this"),
            };
            gridView.SetValue(RenderSettings.ModeProperty, RenderMode.Server);
            var viewModel = new[] { "ROW 1", "ROW 2", "ROW 3" };
            var html = InvokeLifecycleAndRender(gridView, CreateContext(viewModel));

            Assert.IsTrue(html.Contains("ROW 2"));
            Assert.IsTrue(html.Contains("class=\"lol\""));
        }

        [TestMethod]
        public void RepeaterEmptyData_RenderedHtmlTest_ServerRendering()
        {
            Repeater createRepeater(RenderMode renderMode)
            {
                var repeater = new Repeater() {
                    ItemTemplate = new DelegateTemplate((f, c) => c.Children.Add(new HtmlGenericControl("ITEM_TAG"))),
                    EmptyDataTemplate = new DelegateTemplate((f, c) => c.Children.Add(new HtmlGenericControl("EMPTY_DATA"))),
                    DataSource = new ValueBindingExpression(h => h[0], "$this"),
                    RenderWrapperTag = false
                };
                repeater.SetValue(RenderSettings.ModeProperty, renderMode);
                return repeater;
            }
            var viewModel = new string[] { };
            var clientHtml = InvokeLifecycleAndRender(createRepeater(RenderMode.Client), CreateContext(viewModel));

            Assert.IsTrue(clientHtml.Contains("<ITEM_TAG"));
            Assert.IsTrue(clientHtml.Contains("<EMPTY_DATA"));
            Assert.IsTrue(!clientHtml.Contains("<div"));
            Assert.IsTrue(clientHtml.Contains("<!-- ko "));

            var serverHtml = InvokeLifecycleAndRender(createRepeater(RenderMode.Server), CreateContext(viewModel));
            Assert.IsTrue(serverHtml.Contains("<EMPTY_DATA"));
            Assert.IsTrue(!serverHtml.Contains("<div"));
        }

        [TestMethod]
        public void BindingGroup_EnumProperty()
        {
            var writer = new StringWriter();
            var html = new HtmlWriter(writer, CreateContext(new object()));
            html.AddKnockoutDataBind("tt", new KnockoutBindingGroup() {
                { "test", new TextBox(){ Type = TextBoxType.Date }, TextBox.TypeProperty }
            });
            html.RenderSelfClosingTag("span");
            Assert.AreEqual("<spandata-bind=\"tt:{&#39;test&#39;:&quot;Date&quot;}\"/>", writer.ToString().Replace(" ", ""));
        }
    }
}
