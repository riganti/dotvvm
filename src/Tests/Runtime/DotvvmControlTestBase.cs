using System;
using System.IO;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public abstract class DotvvmControlTestBase
    {
        protected DotvvmConfiguration Configuration { get; private set; }
        protected BindingCompilationService BindingService { get; private set; }

        [TestInitialize]
        public virtual void Initialize()
        {
            Configuration = DotvvmTestHelper.DefaultConfig;
            BindingService = Configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
        }

        protected TestDotvvmRequestContext CreateContext(object viewModel, DotvvmConfiguration configuration = null)
        {
            configuration = configuration ?? Configuration;
            return new TestDotvvmRequestContext() {
                Configuration = configuration,
                ResourceManager = new ResourceManager(configuration.Resources),
                ViewModel = viewModel
            };
        }

        protected static string InvokeLifecycleAndRender(DotvvmControl control, TestDotvvmRequestContext context)
        {
            var view = context.View = new DotvvmView();
            view.Children.Add(control);

            return InvokeLifecycleAndRender(view, context);
        }

        protected static string InvokeLifecycleAndRender(DotvvmView view, TestDotvvmRequestContext context)
        {
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

        protected Func<string> CreateControlRenderer(string control, object viewModel)
        {
            var view = $"@viewModel {viewModel.GetType().FullName} \n {control}";

            var builder = CreateDotvvmViewBuilder(view);

            var context = CreateContext(viewModel);
            return () => InvokeLifecycleAndRender(builder.BuildView(context), context);
        }

        protected DefaultDotvvmViewBuilder CreateDotvvmViewBuilder(string view)
        {
            var markupLoader = new StaticContentMarkupLoader(view);

            return new DefaultDotvvmViewBuilder(markupLoader,
                new DefaultControlBuilderFactory(Configuration, markupLoader, Configuration.ServiceProvider.GetRequiredService<CompiledAssemblyCache>()),
                Configuration.Markup);
        }

        public class StaticContentMarkupLoader : IMarkupFileLoader
        {
            private readonly string content;

            public StaticContentMarkupLoader(string content)
            {
                this.content = content;
            }

            public MarkupFile GetMarkup(DotvvmConfiguration configuration, string virtualPath) =>
                new MarkupFile("test.dothtml", "test/test.dothtml", content);

            public string GetMarkupFileVirtualPath(IDotvvmRequestContext context) => content;
        }
    }
}
