using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Allows the <see cref="DotvvmControl"/> to perform additional checks in the event validation phase (e.g. make sure that a button which is disabled, cannot invoke the click event).
    /// </summary>
    public interface IEventValidationHandler
    {

        /// <summary>
        /// Determines whether it is legal to invoke a command on the specified property.
        /// </summary>
        bool ValidateCommand(DotvvmProperty targetProperty);

    }
}