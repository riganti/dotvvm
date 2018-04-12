using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Translators
{
   
    public interface ITranslator<in TInput>
    {
        bool CanTranslate(TInput input);
        ISyntaxNode Translate(TInput input);
    }
}
