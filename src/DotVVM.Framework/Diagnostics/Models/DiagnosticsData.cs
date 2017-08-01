using System;
using System.Text;
using DotVVM.Framework.Hosting;


namespace DotVVM.Framework.Diagnostics.Models
{
    public class DiagnosticsData
    {
        public HttpRequestDiagnostics RequestDiagnostics { get; set; }
        public HttpResponseDiagnostics ResponseDiagnostics { get; set; }
        public long TotalDuration { get; set; }
    }
}
