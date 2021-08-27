using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Inference.Results
{
    internal class LambdaTypeInferenceResult : ITypeInferenceResult
    {
        public bool Result { get; private set; }
        public Type? Type { get; private set; }
        public Type[]? Parameters { get; private set; }

        public LambdaTypeInferenceResult(bool result, Type? type = null, Type[]? parameters = null)
        {
            Result = result;
            Type = type;
            Parameters = parameters;
        }
    }
}
