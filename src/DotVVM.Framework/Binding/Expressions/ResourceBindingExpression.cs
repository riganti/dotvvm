using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(OriginalString = BindingCompilationRequirementType.StronglyRequire, Delegate = BindingCompilationRequirementType.StronglyRequire)]
    [BindingCompilation]
    public class ResourceBindingExpression : BindingExpression, IStaticValueBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBindingExpression"/> class.
        /// </summary>
        public ResourceBindingExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBindingExpression"/> class.
        /// </summary>
        public ResourceBindingExpression(CompiledBindingExpression expression) : base(expression)
        {
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public object Evaluate(Controls.DotvvmBindableObject control, DotvvmProperty property)
        {
            return ExecDelegate(control, true);
        }
    }
}