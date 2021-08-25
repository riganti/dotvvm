using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics.Models
{
    public class ResponseDiagnostics
    {
        public int StatusCode { get; set; }
        public IList<HttpHeaderItem>? Headers { get; set; }
        public string? ViewModelJson { get; set; }
        public string? ViewModelDiff { get; set; }
        public long ResponseSize { get; set; }
        public long CompressedResponseSize { get; set; }
        public string? ExceptionStackTrace { get; set; }
    }
}
