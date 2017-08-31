using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Hosting;


namespace DotVVM.Framework.Diagnostics.Models
{

    public class DiagnosticsInformation
    {
        public RequestDiagnostics RequestDiagnostics { get; set; }
        public ResponseDiagnostics ResponseDiagnostics { get; set; }
        public IList<EventTiming> EventTimings { get; set; } 
        public long TotalDuration { get; set; }
    }
}
