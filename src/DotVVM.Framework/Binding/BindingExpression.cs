using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public abstract class BindingExpression
    {

        /// <summary>
        /// Gets or sets the expression.
        /// </summary>
        public string Expression { get; set; }


        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public abstract object Evaluate(DotvvmBindableControl control, DotvvmProperty property);

        /// <summary>
        /// Translates the binding to client script.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="property"></param>
        public abstract string TranslateToClientScript(DotvvmBindableControl control, DotvvmProperty property);



        /// <summary>
        /// Initializes a new instance of the <see cref="BindingExpression"/> class.
        /// </summary>
        public BindingExpression(string expression) : this()
        {
            Expression = expression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingExpression"/> class.
        /// </summary>
        public BindingExpression()
        {
        }



        public virtual BindingExpression Clone()
        {
            return (BindingExpression)Activator.CreateInstance(GetType(), Expression);
        }
    }
}
