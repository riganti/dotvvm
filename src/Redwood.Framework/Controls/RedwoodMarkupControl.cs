using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A base class for all controls with RWHTML markup.
    /// </summary>
    public abstract class RedwoodMarkupControl : HtmlGenericControl
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodMarkupControl"/> class.
        /// </summary>
        public RedwoodMarkupControl() : base("div")
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            SetValue(Internal.IsControlBindingTargetProperty, true);
        }

    }
}
