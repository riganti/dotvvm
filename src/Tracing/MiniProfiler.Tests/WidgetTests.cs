using System;
using System.IO;
using Xunit;

namespace DotVVM.Tracing.MiniProfiler.Tests
{
    public class ResourcesTests
    {
        [Fact]
        public void Widget_InlineJavascript()
        {
            using var reader = new StreamReader(typeof(MiniProfilerWidget).Assembly
                .GetManifestResourceStream(MiniProfilerWidget.IntegrationJSEmbeddedResourceName));
            var data = reader.ReadToEnd();
            Assert.NotEmpty(data);
        }
    }
}
