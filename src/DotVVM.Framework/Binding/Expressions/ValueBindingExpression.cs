using System;
using System.Collections;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;

namespace DotVVM.Framework.Binding.Expressions
{
    /// <summary>
    /// A binding that gets the value from a viewmodel property.
    /// </summary>
    [BindingCompilationRequirements(Delegate = BindingCompilationRequirementType.StronglyRequire,
        OriginalString = BindingCompilationRequirementType.IfPossible,
        Javascript = BindingCompilationRequirementType.StronglyRequire,
        Expression = BindingCompilationRequirementType.IfPossible,
        UpdateDelegate = BindingCompilationRequirementType.IfPossible)]
    [BindingCompilation]
    public class ValueBindingExpression : BindingExpression, IUpdatableValueBinding, IValueBinding
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBindingExpression"/> class.
        /// </summary>
        public ValueBindingExpression()
        {
        }

        public ValueBindingExpression(Func<object[], object> func, string javascript)
            : this((h, c) => func(h), javascript)
        { }

        public ValueBindingExpression(CompiledBindingExpression.BindingDelegate func, string javascript = null, CompiledBindingExpression.BindingUpdateDelegate updateFunc = null)
            : base(new CompiledBindingExpression()
            {
                Delegate = func,
                Javascript = javascript,
                UpdateDelegate = updateFunc
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueBindingExpression"/> class.
        /// </summary>
        public ValueBindingExpression(CompiledBindingExpression expression)
            : base(expression)
        {
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public virtual object Evaluate(DotvvmBindableObject control, DotvvmProperty property)
        {
            return ExecDelegate(control, property != DotvvmBindableObject.DataContextProperty);
        }

        /// <summary>
        /// Updates the viewModel with the new value.
        /// </summary>
        public virtual void UpdateSource(object value, DotvvmBindableObject control, DotvvmProperty property)
        {
            ExecUpdateDelegate(control, value, property != DotvvmBindableObject.DataContextProperty);
        }
        public string GetKnockoutBindingExpression()
        {
            return Javascript;
        }

        #region Helpers

        public static readonly ValueBindingExpression ThisBinding
             = new ValueBindingExpression(vm => vm[0], "$rawData");

        public ValueBindingExpression MakeListIndexer(int index)
        {
            return new ValueBindingExpression((vm, control) => ((IList)Delegate(vm, control))[index], JavascriptCompilationHelper.AddIndexerToViewModel(Javascript, index));
        }

        public ValueBindingExpression MakeKoContextIndexer()
        {
            return new ValueBindingExpression(null, JavascriptCompilationHelper.AddIndexerToViewModel(Javascript, "$index"));
        }

        #endregion
    }
}