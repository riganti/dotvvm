using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics.Models
{
    public class HttpResponseDiagnostics
    {
        public int StatusCode { get; set; }
        public IList<HttpHeaderItem> Headers { get; set; }
        public string ViewModelJson { get; set; }
        public string ViewModelDiff { get; set; }
        public static HttpResponseDiagnostics FromDotvvmRequestContext(IDotvvmRequestContext request)
        {
            return new HttpResponseDiagnostics
            {
                StatusCode = request.HttpContext.Response.StatusCode,
                Headers = request.HttpContext.Response.Headers.Select(pair => HttpHeaderItem.FromKeyValuePair(pair))
                    .ToList(),
                ViewModelJson = request.ViewModelJson?.GetValue("viewModel")?.ToString(),
                ViewModelDiff = request.ViewModelJson?.GetValue("viewModelDiff")?.ToString()
            };
        }
    }
}