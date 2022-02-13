using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace DotVVM.Framework.Controls.DynamicData.Annotations
{
    /// <summary>
    /// Provides information about the context in which the field is being rendered.
    /// </summary>
    public interface IViewContext
    {
        
        /// <summary>
        /// Gets the name of the current view.
        /// </summary>
        string ViewName { get; }

        /// <summary>
        /// Gets the name of the current field group.
        /// </summary>
        string GroupName { get; }

    }
}
