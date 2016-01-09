using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding
{
    public abstract class BindingExpression: IBinding
    {

        public string OriginalString { get; set; }
        public string Javascript { get; set; }
        public Expression ExpressionTree { get; set; }
        public CompiledBindingExpression.BindingDelegate Delegate { get; set; }
        public CompiledBindingExpression.BindingUpdateDelegate UpdateDelegate { get; set; }
        public string BindingId { get; set; }
        public ActionFilterAttribute[] ActionFilters { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingExpression"/> class.
        /// </summary>
        public BindingExpression(CompiledBindingExpression compiledBinding) : this()
        {
            OriginalString = compiledBinding.OriginalString;
            Javascript = compiledBinding.Javascript;
            ExpressionTree = compiledBinding.Expression;
            Delegate = compiledBinding.Delegate;
            UpdateDelegate = compiledBinding.UpdateDelegate;
            BindingId = compiledBinding.Id;
            ActionFilters = compiledBinding.ActionFilters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingExpression"/> class.
        /// </summary>
        public BindingExpression()
        {
        }

        protected void ExecUpdateDelegate(DotvvmBindableObject contextControl, object value, bool seeThis, bool setRootControl = false)
        {
            var dataContexts = GetDataContexts(contextControl, seeThis);
            var control = setRootControl ? GetRootControl(contextControl) : null;
            UpdateDelegate(dataContexts, control, value);
        }

        protected object ExecDelegate(DotvvmBindableObject contextControl, bool seeThis, bool setRootControl = false)
        {
            var dataContexts = GetDataContexts(contextControl, seeThis);
            var control = setRootControl ? GetRootControl(contextControl) : null;
            try
            {
                return Delegate(dataContexts, control);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        private DotvvmBindableObject GetRootControl(DotvvmBindableObject control)
        {
            return control.GetClosestControlBindingTarget();
        }

        /// <summary>
        /// Gets all data context on the path to root
        /// </summary>
        public static object[] GetDataContexts(DotvvmBindableObject contextControl, bool seeThis)
        {
            if (!seeThis)
            {
                contextControl = contextControl.Parent;
                if (contextControl == null)
                {
                    return new object[0];
                }
            }

            return new [] { contextControl }.Concat(contextControl.GetAllAncestors())
                .Where(c => c.IsPropertySet(DotvvmBindableObject.DataContextProperty, false))
                .Select(c => c.GetValue(DotvvmBindableObject.DataContextProperty, false))
                .ToArray();
        }



        public virtual BindingExpression Clone()
        {
            return (BindingExpression)Activator.CreateInstance(GetType(), new CompiledBindingExpression()
            {
                OriginalString = OriginalString,
                Javascript = Javascript,
                Expression = ExpressionTree,
                Delegate = Delegate,
                ActionFilters = ActionFilters,
                Id = BindingId,
                UpdateDelegate = UpdateDelegate
            });
        }
    }
}
