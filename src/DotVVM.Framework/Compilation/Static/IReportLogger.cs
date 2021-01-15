#nullable enable

using System.Collections.Generic;
using System.IO;

namespace DotVVM.Framework.Compilation.Static
{
    internal interface IReportLogger
    {
        void Log(Stream stream, IEnumerable<Report> reports);
    }
}
