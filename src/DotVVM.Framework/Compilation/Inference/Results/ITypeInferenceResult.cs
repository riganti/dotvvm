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
        public bool Result { get; }
        public Type? Type { get; }
    }
}
