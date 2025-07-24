using System;

namespace DotVVM.Framework.Compilation.Inference.Results
{
    internal record ConstructorTypeInferenceResult(Type? Type, bool Result) : ITypeInferenceResult;
}
