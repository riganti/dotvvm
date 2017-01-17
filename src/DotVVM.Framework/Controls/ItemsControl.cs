using DotVVM.Framework.Binding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using System.Reflection;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A common base for all controls that operate on collection.
    /// </summary>
    public abstract class ItemsControl : HtmlGenericControl
    {
        /// <summary>
        /// Gets or sets the source collection or a GridViewDataSet that contains data in the control.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public object DataSource
        {
            get { return GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly DotvvmProperty DataSourceProperty =
            DotvvmProperty.Register<object, ItemsControl>(t => t.DataSource, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsControl"/> class.
        /// </summary>
        public ItemsControl()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsControl"/> class.
        /// </summary>
        public ItemsControl(string tagName) : base(tagName)
        {
        }

        /// <summary>
        /// Gets the data source binding.
        /// </summary>
        protected IValueBinding GetDataSourceBinding()
        {
            var binding = GetValueBinding(DataSourceProperty);
            if (binding == null)
            {
                throw new DotvvmControlException(this, $"The DataSource property of the '{GetType().Name}' control must be set!");
            }
            return binding;
        }

        protected ValueBindingExpression GetItemBinding(int index)
        {
            return GetValueBinding(DataSourceProperty).CastTo<ValueBindingExpression>().MakeListIndexer(index);
        }

        public static IEnumerable GetIEnumerableFromDataSource(object dataSource)
        {
            if (dataSource == null)
            {
                return null;
            }
            if (dataSource is IEnumerable)
            {
                return (IEnumerable)dataSource;
            }
            if (dataSource is IGridViewDataSet)
            {
                return ((IGridViewDataSet)dataSource).Items;
            }
            throw new NotSupportedException($"The object of type '{dataSource.GetType()}' is not supported in the DataSource property!");
        }

        protected ParametrizedCode WrapJavascriptDataSourceAccess(ParametrizedCode expression)
        {
            // T+ JsTree compile time processing
            return new ParametrizedCode.Builder {
               "dotvvm.evaluator.getDataSourceItems(", expression, ")"
            }.Build(OperatorPrecedence.Max);
            //return "dotvvm.evaluator.getDataSourceItems(" + expression + ")";
        }

        protected ParametrizedCode GetForeachDataBindJavascriptExpression()
        {
            var binding = GetDataSourceBinding();
            return typeof(IList).IsAssignableFrom(binding.ResultType) ?
                   binding.KnockoutExpression :
                   WrapJavascriptDataSourceAccess(binding.KnockoutExpression);
        }

        protected string GetPathFragmentExpression()
        {
            return GetDataSourceBinding().GetKnockoutBindingExpression(this);
        }
    }
}