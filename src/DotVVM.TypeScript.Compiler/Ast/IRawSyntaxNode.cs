using System;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IRawSyntaxNode : IExpressionSyntax
    {
        string Value { get; }
    }
}
