#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Inference.Results
{
    internal interface ITypeInferenceResult
    {
        bool Result { get; }
        Type? Type { get; }
    }
}
