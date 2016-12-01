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
            return new TestDotvvmRequestContext()
            {
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
            var gridView = new GridView()
            {
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
    }
}
