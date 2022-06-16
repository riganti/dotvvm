using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using DotVVM.Framework.Hosting;


namespace DotVVM.Framework.Diagnostics.Models
{

    public class DiagnosticsInformation
    {
        public DiagnosticsInformation(RequestDiagnostics requestDiagnostics, ResponseDiagnostics responseDiagnostics, IList<EventTiming> eventTimings, long totalDuration)
        {
            RequestDiagnostics = requestDiagnostics;
            ResponseDiagnostics = responseDiagnostics;
            EventTimings = eventTimings;
            TotalDuration = totalDuration;
        }

        public RequestDiagnostics RequestDiagnostics { get; set; }
        public ResponseDiagnostics ResponseDiagnostics { get; set; }
        public IList<EventTiming> EventTimings { get; set; } 
        public long TotalDuration { get; set; }
    }
}
