using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Compiler
{
    public class FileCompilationResult
    {
        public Dictionary<int, ControlCompilationInfo> Controls { get; set; } = new Dictionary<int, ControlCompilationInfo>();
        public List<Exception> Errors { get; set; } = new List<Exception>();
    }
}
