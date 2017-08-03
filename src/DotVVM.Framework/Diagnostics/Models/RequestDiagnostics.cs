using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Diagnostics.Models
{
    public class RequestDiagnostics
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public IList<HttpHeaderItem> Headers { get; set; }
        public string ViewModelJson { get; set; }
    }
}