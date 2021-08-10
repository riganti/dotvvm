using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics.Models
{

    public enum RequestType
    {
        Get,
        Command,
        StaticCommand
    }

    public class RequestDiagnostics
    {
        public RequestType RequestType { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public IList<HttpHeaderItem> Headers { get; set; }
        public string ViewModelJson { get; set; }
    }
}