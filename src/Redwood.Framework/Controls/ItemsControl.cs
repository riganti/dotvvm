using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A common base for all controls that operate on collection.
    /// </summary>
    public abstract class ItemsControl : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets whether the items of the control are rendered on the server.
        /// </summary>
        public bool RenderOnServer
        {
            get { return (bool)GetValue(RenderOnServerProperty); }
            set { SetValue(RenderOnServerProperty, value); }
        }
        public static readonly RedwoodProperty RenderOnServerProperty =
            RedwoodProperty.Register<bool, Repeater>(t => t.RenderOnServer, false);


        /// <summary>
        /// Gets or sets the source collection that is used.
        /// </summary>
        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public static readonly RedwoodProperty DataSourceProperty =
            RedwoodProperty.Register<IEnumerable, ItemsControl>(t => t.DataSource, null);


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
        protected ValueBindingExpression GetDataSourceBinding()
        {
            var binding = GetValueBinding(DataSourceProperty);
            if (binding == null)
            {
                throw new Exception(string.Format("The DataSource property of the {0} control must be set!", GetType().Name));
            }
            return binding;
        }
    }
}