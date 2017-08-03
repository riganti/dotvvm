using System;
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
        public long ResponseSize { get; set; }
        public string ExceptionStackTrace { get; set; }

        internal static HttpResponseDiagnostics FromDotvvmRequestContext(IDotvvmRequestContext request)
        {
            return new HttpResponseDiagnostics
            {
                StatusCode = request.HttpContext.Response.StatusCode,
                Headers = request.HttpContext.Response.Headers.Select(pair => HttpHeaderItem.FromKeyValuePair(pair))
                    .ToList(),
                ViewModelJson = request.ViewModelJson?.GetValue("viewModel")?.ToString(),
                ViewModelDiff = request.ViewModelJson?.GetValue("viewModelDiff")?.ToString(),
                ResponseSize = GetRequestContentLength(request)
            };
        }

        private static long GetRequestContentLength(IDotvvmRequestContext request)
        {
            var dotvvmPresenter = request.Presenter as DotvvmPresenter;
            var diagnosticsRenderer = dotvvmPresenter?.OutputRenderer as DiagnosticsRenderer;
            if (diagnosticsRenderer != null)
            {
                return diagnosticsRenderer.ContentLength;
            }
            return 0;
        }
    }
}