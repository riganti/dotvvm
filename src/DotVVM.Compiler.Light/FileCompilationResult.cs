using System;
using System.Collections.Generic;

namespace Comp
{
    public class FileCompilationResult
    {
        public Dictionary<int, ControlCompilationInfo> Controls { get; set; } = new Dictionary<int, ControlCompilationInfo>();
        public List<Exception> Errors { get; set; } = new List<Exception>();
    }
}