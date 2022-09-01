using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Interface for controls which will accept any attribute and stores it in the Attributes collection.
    /// </summary>
    public interface IControlWithHtmlAttributes
    {
        /// <summary> A dictionary of html attributes that are rendered on this control's html tag. </summary>
        VirtualPropertyGroupDictionary<object?> Attributes { get; } 
    }
}
