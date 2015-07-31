using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(Delegate = BindingCompilationRequirementType.StronglyRequire,
        Javascript = BindingCompilationRequirementType.StronglyRequire,
        ActionFilters = BindingCompilationRequirementType.IfPossible)]
    [CommandPostbackJsCompile]
    public class CommandBindingExpression : BindingExpression, ICommandBinding
    {
        public CommandBindingExpression()
        {
        }

        public CommandBindingExpression(Action<object[]> command, string id)
            : this((h, o) => { command(h); return null; }, id)
        { }

        public CommandBindingExpression(CompiledBindingExpression.BindingDelegate command, string id)
            : base(new CompiledBindingExpression() { Delegate = command, Id = id, Javascript = $"dotvvm.postbackScript('{ id }')" })
        {
        }

        public CommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        {
        }


        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public virtual object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            return ExecDelegate(control, property != DotvvmBindableControl.DataContextProperty);
        }

        public string GetCommandJavascript()
        {
            return Javascript;
        }
    }
}