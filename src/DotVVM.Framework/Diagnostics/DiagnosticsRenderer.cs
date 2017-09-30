using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Diagnostics
{
    class DiagnosticsRenderer : DefaultOutputRenderer
    {
        protected override string RenderPage(IDotvvmRequestContext context, DotvvmView view)
        {
            var html = base.RenderPage(context, view);
            if (context.Configuration.Debug && context.Services.GetService<DiagnosticsRequestTracer>() is DiagnosticsRequestTracer tracer)
            {
                tracer.LogResponseSize(GetCompressedSize(html), Encoding.UTF8.GetByteCount(html));
            }
            return html;
        }

        public override Task WriteViewModelResponse(IDotvvmRequestContext context, DotvvmView view)
        {
            var viewModelJson = context.GetSerializedViewModel();
            if (context.Configuration.Debug && context.Services.GetService<DiagnosticsRequestTracer>() is DiagnosticsRequestTracer tracer)
            {
                tracer.LogResponseSize(GetCompressedSize(viewModelJson), Encoding.UTF8.GetByteCount(viewModelJson));
            }
            return base.WriteViewModelResponse(context, view);
        }

        private long GetCompressedSize(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            using (var memoryStream = new MemoryStream())
            using (var zipStream = new GZipStream(memoryStream, CompressionLevel.Fastest))
            {
                zipStream.Write(bytes, 0, bytes.Length);
                zipStream.Flush();
                memoryStream.Flush();
                return memoryStream.Length;
            }
        }
    }
}
