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
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Javascript;
using System.Collections;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class ControlRenderedHtmlTests
    {
        private DotvvmConfiguration configuration;
        private BindingCompilationService bindingService;

        [TestInitialize]
        public void INIT()
        {
            this.configuration = DotvvmConfiguration.CreateDefault();
            this.bindingService = configuration.ServiceLocator.GetService<BindingCompilationService>();
        }


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
                    new GridViewTextColumn() { HeaderCssClass = "lol", HeaderText="Header Text", ValueBinding = ValueBindingExpression.CreateBinding(bindingService, h => (object)h[0]) }
                },
                DataSource = ValueBindingExpression.CreateBinding(bindingService, h => (IList)h[0]),
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
                    DataSource = ValueBindingExpression.CreateThisBinding<string[]>(configuration.ServiceLocator.GetService<BindingCompilationService>()),
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
    }
}
