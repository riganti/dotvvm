using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public interface IOperationTranslator<in TInput> : ITranslator<TInput> where TInput : IOperation
    {
    }
}
