#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary> Object that contains DotvvmProperties. These properties may contain bindings, but the bindings can't be evaluated on this object. </summary>
    public interface IWithDotvvmProperties
    {
        /// <summary> Gets the collection of control property values. </summary>
        DotvvmPropertyDictionary Properties { get; }
    }
}
