using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(Delegate = BindingCompilationRequirementType.StronglyRequire,
        Javascript = BindingCompilationRequirementType.StronglyRequire,
        ActionFilters = BindingCompilationRequirementType.IfPossible)]
    [CommandPostbackJsCompile]
    public class CommandBindingExpression : BindingExpression
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
        public override object Evaluate(DotvvmBindableControl control, DotvvmProperty property)
        {
            return ExecDelegate(control, property != DotvvmBindableControl.DataContextProperty);
        }

        /// <summary>
        /// Translates the binding to client script.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="property"></param>
        public override string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property)
        {
            throw new NotSupportedException("can't translate command to javascript");
        }
    }
}