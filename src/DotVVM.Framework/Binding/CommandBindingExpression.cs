using System;
using DotVVM.Framework.Controls;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Represents typical command binding delegate, quivalent to Func&lt;Task&gt;
    /// </summary>
    public delegate Task Command();

    [BindingCompilationRequirements(Delegate = BindingCompilationRequirementType.StronglyRequire,
        Javascript = BindingCompilationRequirementType.StronglyRequire,
        ActionFilters = BindingCompilationRequirementType.IfPossible)]
    [CommandBindingCompilation]
    public class CommandBindingExpression : BindingExpression, ICommandBinding
    {
        public CommandBindingExpression() { }

        public CommandBindingExpression(Action<object[]> command, string id)
            : this((h, o) => (Action)(() => command(h)), id)
        { }

        public CommandBindingExpression(Delegate command, string id)
            : this((h, o) => command, id)
        { }

        public CommandBindingExpression(CompiledBindingExpression.BindingDelegate command, string id)
            : base(new CompiledBindingExpression { Delegate = command, Id = id, Javascript = $"dotvvm.postbackScript('{ id }')" })
        { }

        public CommandBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        { }


        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public object Evaluate(DotvvmBindableControl control, DotvvmProperty property, params object[] args)
        {
            var action = GetCommandDelegate(control, property);
            if (action is Command) return (action as Command)();
            if (action is Action) { (action as Action)(); return null; }
            return action.DynamicInvoke(args);
        }

        public virtual Delegate GetCommandDelegate(DotvvmBindableControl control, DotvvmProperty property)
        {
            return (Delegate)ExecDelegate(control, property != DotvvmBindableControl.DataContextProperty);
        }

        public string GetCommandJavascript()
        {
            return Javascript;
        }
    }
}