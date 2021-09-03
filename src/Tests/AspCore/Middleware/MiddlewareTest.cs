using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Moq;
using System.Threading;
using DotVVM.Framework.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.AspCore.Middleware
{
    [TestClass]
    public class MiddlewareTest
    {
        public const string FinalFunction = "final";
        public const string AfterFunction = "after";
        public const string BeforeFunction = "before";

        private IDotvvmRequestContext _requestContext;
        private Stream _stream;
        
        [TestInitialize]
        public void Initialize()
        {
            var mockResponse = new Mock<IHttpResponse>();
            _stream = new MemoryStream();

            mockResponse
                .Setup(p => p.WriteAsync(It.IsAny<string>()))
                .Returns<string>(
                async (text) =>
                {
                    var writer = new StreamWriter(_stream) { AutoFlush = true };
                    await writer.WriteAsync(text);
                });

            var mockContext = new Mock<IHttpContext>();
            mockContext.Setup(m => m.Response).Returns(mockResponse.Object);

            var configuration = DotvvmConfiguration.CreateDefault();
            _requestContext = new DotvvmRequestContext(mockContext.Object, configuration, configuration.ServiceProvider);
        }


        [TestMethod]
        public async Task TestFinalFunction()
        {
            await _requestContext.HttpContext.Response.WriteAsync(FinalFunction);
            Assert.AreEqual(FinalFunction, ReadResponseBody());
        }

        [TestMethod]
        public async Task TestBeforeMiddleware()
        {

            var middleware = new BeforeMiddleware();
            await middleware.Handle(_requestContext, async context =>
            {
                await context.HttpContext.Response.WriteAsync(FinalFunction);
            });

            Assert.AreEqual(BeforeFunction + FinalFunction, ReadResponseBody());
        }

        [TestMethod]
        public async Task TestAfterMiddleware()
        {
            var middleware = new AfterMiddleware();
            await middleware.Handle(_requestContext, async context =>
            {
                await context.HttpContext.Response.WriteAsync(FinalFunction);
            });

            Assert.AreEqual(FinalFunction + AfterFunction, ReadResponseBody());
        }

        [TestMethod]
        public async Task TestAllMiddleware()
        {
            var before = new BeforeMiddleware();
            var after = new AfterMiddleware();

            await before.Handle(_requestContext,
                async c =>
                {
                    await after.Handle(c,
                        async context =>
                        {
                            await context.HttpContext.Response.WriteAsync(FinalFunction);
                        });
                });

            Assert.AreEqual(BeforeFunction + FinalFunction + AfterFunction, ReadResponseBody());
        }


        private string ReadResponseBody()
        {
            _stream.Position = 0;
            return new StreamReader(_stream).ReadToEnd();
        }
    }
}
