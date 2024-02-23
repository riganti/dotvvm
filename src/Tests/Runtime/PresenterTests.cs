using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckTestOutput;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Runtime
{
    [TestClass]
    public class PresenterTests
    {
        static readonly DotvvmConfiguration configuration;
        static readonly DotvvmPresenter presenter;

        static readonly string commandId;

        static PresenterTests()
        {
            var files = new FakeMarkupFileLoader();
            configuration = DotvvmTestHelper.CreateConfiguration(s => {
                s.AddSingleton<IMarkupFileLoader>(files);
            });

            files.MarkupFiles["Default.dothtml"] = $$"""
                @viewModel {{typeof(TestViewModel)}}

                <dot:Button Click={staticCommand: IntProp = {{typeof(PresenterTests).FullName}}.StaticCommand()} />
                <dot:Button Click={command: IntProp = 9876} />
            """;
            configuration.RouteTable.Add("Default", "", "Default.dothtml", null);
            configuration.Runtime.MaxPostbackSizeBytes = 1024 * 512;

            configuration.Freeze();
            presenter = (DotvvmPresenter)configuration.ServiceProvider.GetRequiredService<IDotvvmPresenter>();


            var controlBuilderFactory = configuration.ServiceProvider.GetRequiredService<IControlBuilderFactory>();
            var view = controlBuilderFactory.GetControlBuilder("Default.dothtml").builder.Value.BuildControl(controlBuilderFactory, configuration.ServiceProvider);

            var command = view.GetAllDescendants().OfType<Button>().Select(b => b.GetValueRaw(Button.ClickProperty) as CommandBindingExpression).First(x => x is not null);
            commandId = command.BindingId;
        }

        [TestMethod]
        public async Task PostbackSizeLimit()
        {
            var context = DotvvmTestHelper.CreateContext(configuration, configuration.RouteTable["Default"], DotvvmRequestType.Command);
            var httpContext = (TestHttpContext)context.HttpContext;
            httpContext.Request.Headers["Content-Type"] = new [] {"application/json"};
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(new string('[', 1024 * 600)));

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => presenter.ProcessRequest(context));
            XAssert.Contains("The stream is limited to", ex.Message);
            XAssert.Contains("To increase the maximum request size, use the DotvvmConfiguration.Runtime.MaxPostbackSizeBytes", ex.Message);
        }

        [TestMethod]
        public async Task PostbackSizeLimitCompressed()
        {
            var context = DotvvmTestHelper.CreateContext(configuration, configuration.RouteTable["Default"], DotvvmRequestType.Command);
            var httpContext = (TestHttpContext)context.HttpContext;
            httpContext.Request.Headers["Content-Type"] = new [] {"application/json"};
            httpContext.Request.Headers["Content-Encoding"] = new [] {"gzip"};
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = Compress(new string('[', 1024 * 600));

            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => presenter.ProcessRequest(context));
            XAssert.Contains("The stream is limited to", ex.Message);
            XAssert.Contains("To increase the maximum request size, use the DotvvmConfiguration.Runtime.MaxPostbackSizeBytes", ex.Message);
        }

        [TestMethod]
        public async Task PostbackAcceptsCompression()
        {
            var request = $$"""
            {"currentPath":[],"command":"{{commandId}}","controlUniqueId":"","validationTargetPath":"/","renderedResources":[],"commandArgs":[],"knownTypeMetadata":[],"viewModel":{"$csrfToken":"Not a CSRF token."} }
            """;
            var context = DotvvmTestHelper.CreateContext(configuration, configuration.RouteTable["Default"], DotvvmRequestType.Command);
            var httpContext = (TestHttpContext)context.HttpContext;
            httpContext.Request.Headers["Content-Type"] = new [] {"application/json"};
            httpContext.Request.Headers["Content-Encoding"] = new [] {"gzip"};
            httpContext.Request.Method = "POST";
            httpContext.Request.Body = Compress(request);

            await presenter.ProcessRequest(context);

            Assert.AreEqual(9876, ((TestViewModel)context.ViewModel).IntProp);
        }

        Stream Compress(string data)
        {
            var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
            using (var sw = new StreamWriter(gzip, new UTF8Encoding(false)))
            {
                sw.Write(data);
            }
            ms.Position = 0;
            return ms;
        }

        [AllowStaticCommand]
        public static int StaticCommand()
        {
            return 123;
        }
    }
}
