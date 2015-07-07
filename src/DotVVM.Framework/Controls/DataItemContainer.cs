using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// User in <see cref="ItemsControl" /> to wrap each data item and generate its unique ID.
    /// </summary>
    public class DataItemContainer : DotvvmBindableControl
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DataItemContainer"/> class.
        /// </summary>
        public DataItemContainer()
        {
            SetValue(Internal.IsNamingContainerProperty, true);
        }


        /// <summary>
        /// Gets or sets the index of the data item in the data source control.
        /// </summary>
        public int? DataItemIndex 
        {
            get
            {
                var value = GetValue(Internal.UniqueIDProperty);
                return value == null ? (int?)null : int.Parse(value as string);
            }
            set { SetValue(Internal.UniqueIDProperty, value != null ? value.ToString() : null); }
        }
    }
}