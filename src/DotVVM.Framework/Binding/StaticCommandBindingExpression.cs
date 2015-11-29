using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using System.Reflection;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(Javascript = BindingCompilationRequirementType.StronglyRequire)]
    [StaticCommandBindingCompilation]
    public class StaticCommandBindingExpression : CommandBindingExpression
    {
        public StaticCommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        { }

        public override Delegate GetCommandDelegate(DotvvmBindableObject control, DotvvmProperty property)
        {
            throw new NotImplementedException();
        }
    }
}
