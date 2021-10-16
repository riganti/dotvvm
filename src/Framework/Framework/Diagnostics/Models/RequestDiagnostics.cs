using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics.Models
{

    public enum RequestType
    {
        Init,
        Get,
        Command,
        StaticCommand
    }

    public class RequestDiagnostics
    {
        public RequestDiagnostics(RequestType requestType, string method, string url, IEnumerable<HttpHeaderItem> headers, string? viewModelJson)
        {
            RequestType = requestType;
            Method = method;
            Url = url;
            Headers = headers.ToList();
            ViewModelJson = viewModelJson;
        }

        public RequestType RequestType { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public IList<HttpHeaderItem> Headers { get; set; }
        public string? ViewModelJson { get; set; }
    }
}
