using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Diagnostics
{
    class DiagnosticsRenderer : DefaultOutputRenderer
    {
        public long ContentLength { get; private set; }

        protected override string RenderPage(IDotvvmRequestContext context, DotvvmView view)
        {
            var html = base.RenderPage(context, view);
            ContentLength = GetCompressedSize(html);
            return html;
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

        public override Task WriteViewModelResponse(IDotvvmRequestContext context, DotvvmView view)
        {
            var viewModelJson = context.GetSerializedViewModel();
            ContentLength = viewModelJson.Length;
            return base.WriteViewModelResponse(context, view);
        }

    }
}
