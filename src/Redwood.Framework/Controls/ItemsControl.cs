using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A common base for all controls that operate on collection.
    /// </summary>
    public abstract class ItemsControl : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets whether the contents of the control are rendered on the server.
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

    }
}