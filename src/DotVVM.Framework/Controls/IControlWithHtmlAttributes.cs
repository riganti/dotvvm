#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Interface for controls which will accept any attribute and stores it in the Attributes collection.
    /// </summary>
    public interface IControlWithHtmlAttributes
    {

        Dictionary<string, object?> Attributes { get; } 

    }
}
