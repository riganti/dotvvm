using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    public class DataItemContainer : RedwoodBindableControl
    {

        public DataItemContainer()
        {
            SetValue(Internal.IsNamingContainerProperty, true);
        }


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