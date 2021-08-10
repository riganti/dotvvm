using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Compilation
{
    /// <summary>
    /// Tells the DotVVM view compiler that instance of a marked type may be used in DotvvmProperty. It is supposed to be immutable, as it will be shared across all requests and controls with the same property setter.
    /// </summary>
    public class HandleAsImmutableObjectInDotvvmPropertyAttribute : Attribute
    {
    }
}
