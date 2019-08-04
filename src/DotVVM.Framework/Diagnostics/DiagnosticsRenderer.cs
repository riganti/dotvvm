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
        protected override MemoryStream RenderPage(IDotvvmRequestContext context, DotvvmView view)
        {
            var html = base.RenderPage(context, view);
            if (context.Configuration.Debug && context.Services.GetService<DiagnosticsRequestTracer>() is DiagnosticsRequestTracer tracer)
            {
                tracer.LogResponseSize(GetCompressedSize(html.ToArray()), html.Length);
            }
            return html;
        }

        public override Task WriteViewModelResponse(IDotvvmRequestContext context, DotvvmView view)
        {
            if (context.Configuration.Debug && context.Services.GetService<DiagnosticsRequestTracer>() is DiagnosticsRequestTracer tracer)
            {
                var viewModelJson = context.GetSerializedViewModel();
                var vmBytes = Encoding.UTF8.GetBytes(viewModelJson);
                tracer.LogResponseSize(GetCompressedSize(vmBytes), vmBytes.LongLength);
            }
            return base.WriteViewModelResponse(context, view);
        }

        private long GetCompressedSize(byte[] bytes)
        {
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
