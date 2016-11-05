using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using Xunit;
using Xunit.Abstractions;
using System.Threading;

namespace DotVVM.Framework.Tests.AspCore.Middleware
{
    //TODO: CODEREVIEW: Now these tests should at least work as orriginaly intended
    //There is a question however what these really test and if they are useful
    public class MiddlewareTest
    {
        private readonly ITestOutputHelper _output;

        public const string FinalFunction = "final";
        public const string AfterFunction = "after";
        public const string BeforeFunction = "before";

        private IDotvvmRequestContext _requestContext;
        private Stream _stream;

        public MiddlewareTest(ITestOutputHelper output)
        {
            _output = output;
            Initialize();
        }

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

            _requestContext = new DotvvmRequestContext() { HttpContext = mockContext.Object };
        }


        [Fact]
        public async Task TestFinalFuncion()
        {
            await _requestContext.HttpContext.Response.WriteAsync(FinalFunction);
            Assert.Equal(FinalFunction, ReadResponseBody());
        }

        [Fact]
        public async Task TestBeforeMiddleware()
        {

            var middlewere = new BeforeMiddleware();
            await middlewere.Handle(_requestContext, async context =>
            {
                await context.HttpContext.Response.WriteAsync(FinalFunction);
            });

            Assert.Equal(BeforeFunction + FinalFunction, ReadResponseBody());
        }

        [Fact]
        public async Task TestAfterMiddleware()
        {
            var middlewere = new AfterMiddleware();
            await middlewere.Handle(_requestContext, async context =>
            {
                await context.HttpContext.Response.WriteAsync(FinalFunction);
            });

            Assert.Equal(BeforeFunction + FinalFunction, ReadResponseBody());
        }

        [Fact]
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

            Assert.Equal(BeforeFunction + FinalFunction + AfterFunction, ReadResponseBody());
        }


        private string ReadResponseBody()
        {
            _stream.Position = 0;
            return new StreamReader(_stream).ReadToEnd();
        }
    }
}