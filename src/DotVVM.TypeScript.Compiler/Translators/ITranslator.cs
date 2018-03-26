using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;

namespace DotVVM.TypeScript.Compiler.Translators
{
   
    public interface ITranslator<in TInput>
    {
        bool CanTranslate(TInput input);
        TsSyntaxNode Translate(TInput input);
    }
}
