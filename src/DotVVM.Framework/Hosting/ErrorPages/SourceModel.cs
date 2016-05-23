using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class SourceModel
    {
        public string FileName { get; set; }
        public string SystemFileName => FileName?.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        public bool LoadFailed { get; set; }
        public string[] PreLines { get; set; }
        public string CurrentLine { get; set; }
        public string[] PostLines { get; set; }
        public int LineNumber { get; set; }
        public int ErrorColumn { get; set; }
        public int ErrorLength { get; set; }
    }
}
