using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;

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
                throw new Exception(string.Format("The DataSource property of the {0} control must be set!", GetType().Name));
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
            throw new NotSupportedException(string.Format("The object of type {0} is not supported in the DataSource property!", dataSource.GetType()));
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