using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser
{
    public interface ITextRange
    {
        int StartPosition { get; }
        int Length { get; }
        int EndPosition { get; }
    }
}
