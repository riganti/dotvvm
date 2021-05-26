#nullable enable

using System.Collections.Generic;
using System.IO;

namespace DotVVM.Framework.Compilation.Static
{
    internal interface ICompilationReportLogger
    {
        void Log(Stream stream, IEnumerable<CompilationReport> reports);
    }
}
