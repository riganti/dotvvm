using DotVVM.Framework.Binding;
using DotVVM.Framework.Exceptions;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        protected ValueBindingExpression GetItemBinding(IList items, string dataSourceJs, int index)
        {
            return new ValueBindingExpression(new CompiledBindingExpression()
            {
                Delegate = (h, c) => items[index],
                Javascript = JavascriptCompilationHelper.AddIndexerToViewModel(WrapJavascriptDataSourceAccess(dataSourceJs), index, true)
            });
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

        protected string WrapJavascriptDataSourceAccess(string expression)
        {
            return "dotvvm.getDataSourceItems(" + expression + ")";
        }

        protected string GetForeachDataBindJavascriptExpression()
        {
            return WrapJavascriptDataSourceAccess(GetDataSourceBinding().GetKnockoutBindingExpression());
        }

        protected string GetPathFragmentExpression()
        {
            return GetDataSourceBinding().GetKnockoutBindingExpression();
        }
    }
}