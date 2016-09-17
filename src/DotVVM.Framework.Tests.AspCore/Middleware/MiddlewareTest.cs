using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Pipeline;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace DotVVM.Framework.Tests.AspCore.Middleware
{
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
            mockResponse.Setup(p => p.Write(It.IsAny<string>())).Callback((string text) =>
            {
                var writer = new StreamWriter(_stream) {AutoFlush = true};
                writer.Write(text);
            });


            var mockContext = new Mock<IHttpContext>();
            mockContext.Setup(m => m.Response).Returns(mockResponse.Object);

            _requestContext = new DotvvmRequestContext() {HttpContext = mockContext.Object};
        }


        [Fact]
        public async void TestFinalFuncion()
        {
            await new Pipeline<IDotvvmRequestContext>()
                .Send(_requestContext)
                .Through()
                .Then<Task>(async context => { context.HttpContext.Response.Write(FinalFunction); });

            Assert.Equal(FinalFunction, ReadResponseBody());
        }

        [Fact]
        public async void TestBeforeMiddleware()
        {
            await new Pipeline<IDotvvmRequestContext>()
                .Send(_requestContext)
                .Through(typeof(BeforeMiddleware))
                .Then<Task>(async context => { context.HttpContext.Response.Write(FinalFunction); });

            Assert.Equal(BeforeFunction + FinalFunction, ReadResponseBody());
        }

        [Fact]
        public async void TestAfterMiddleware()
        {
            await new Pipeline<IDotvvmRequestContext>()
                .Send(_requestContext)
                .Through(typeof(AfterMiddleware))
                .Then<Task>(async context => { context.HttpContext.Response.Write(FinalFunction); });

            Assert.Equal(FinalFunction + AfterFunction, ReadResponseBody());
        }

        [Fact]
        public async void TestAllMiddleware()
        {
            await new Pipeline<IDotvvmRequestContext>()
                .Send(_requestContext)
                .Through(typeof(BeforeMiddleware), typeof(AfterMiddleware))
                .Then<Task>(async context => { context.HttpContext.Response.Write(FinalFunction); });

            Assert.Equal(BeforeFunction + FinalFunction + AfterFunction, ReadResponseBody());
        }


        private string ReadResponseBody()
        {
            _stream.Position = 0;
            return new StreamReader(_stream).ReadToEnd();
        }
    }
}