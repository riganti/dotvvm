using System.Collections.Generic;
using System.IO;

namespace DotVVM.Compiler
{
    public interface IReportLogger
    {
        void Log(Stream stream, IEnumerable<Report> reports);
    }
}
