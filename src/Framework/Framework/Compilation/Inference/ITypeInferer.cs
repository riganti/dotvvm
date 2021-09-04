using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Inference.Results;

namespace DotVVM.Framework.Compilation.Inference
{
    internal interface ITypeInferer
    {
        void BeginFunctionCall(MethodGroupExpression? target, int argsCount);
        void EndFunctionCall();

        void SetArgument(Expression expression, int index);
        void SetProbedArgumentIndex(int index);

        IFluentInferer Infer(Type? expectedType = null);
    }

    internal interface IFluentInferer
    {
        LambdaTypeInferenceResult Lambda(int argsCount);
    }
}
