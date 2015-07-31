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

        protected void ExecUpdateDelegate(DotvvmBindableControl contextControl, object value, bool seeThis, bool setRootControl = false)
        {
            var dataContexts = GetDataContexts(contextControl, seeThis);
            var control = setRootControl ? GetRootControl(contextControl) : null;
            UpdateDelegate(dataContexts, control, value);
        }

        protected object ExecDelegate(DotvvmBindableControl contextControl, bool seeThis, bool setRootControl = false)
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

        private DotvvmControl GetRootControl(DotvvmBindableControl control)
        {
            return control.GetClosestControlBindingTarget();
        }

        /// <summary>
        /// Gets all data context on the path to root
        /// </summary>
        public static object[] GetDataContexts(DotvvmBindableControl contextControl, bool seeThis)
        {
            var context = seeThis ? contextControl.GetValue(DotvvmBindableControl.DataContextProperty, false) : null;
            return
                (context == null ? new object[0] : new[] { context })
                .Concat(contextControl.GetAllAncestors().OfType<DotvvmBindableControl>()
                .Where(c => c.properties.ContainsKey(DotvvmBindableControl.DataContextProperty))
                .Select(c => c.GetValue(DotvvmBindableControl.DataContextProperty, false)))
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
