using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics.Models
{
    public class HttpRequestDiagnostics
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public IList<HttpHeaderItem> Headers { get; set; }
        public string ViewModelJson { get; set; }

        public static HttpRequestDiagnostics FromDotvvmRequestContext(IDotvvmRequestContext request)
        {
            return new HttpRequestDiagnostics
            {
                Method = request.HttpContext.Request.Method,
                Url = request.HttpContext.Request.Path.Value,
                Headers = request.HttpContext.Request.Headers.Select(HttpHeaderItem.FromKeyValuePair)
                    .ToList(),
                ViewModelJson = request.ReceivedViewModelJson?.GetValue("viewModel")?.ToString()
            };
        }
    }
}