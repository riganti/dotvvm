using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser.Translation;
using System.Reflection;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(Javascript = BindingCompilationRequirementType.StronglyRequire)]
    [StaticCommandJsCompile]
    public class StaticCommandBindingExpression : CommandBindingExpression
    {
        private static ExpressionTranslator translator = new ExpressionTranslator();

        public StaticCommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        { } 

        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            throw new NotImplementedException();
        }

        public override string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property)
        {
            throw new NotImplementedException();
        }

    }
}
