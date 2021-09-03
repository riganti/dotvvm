using DotVVM.Tracing.MiniProfiler.Shared;
using System;
using Xunit;

namespace DotVVM.Tracing.MiniProfiler.Tests
{
    public class ResourcesTests
    {
        [Fact]
        public void Widget_InlineJavascript()
        {
            var data = MiniProfilerJavascriptResourceManager.GetWigetInlineJavascriptContent();
            Assert.NotEmpty(data);
        }
    }
}
