using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Runtime
{
    public interface IControlBuilder
    {
        DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory);
        /// <summary>
        /// Gets required data context for the control
        /// </summary>
        Type DataContextType { get; }
        /// <summary>
        /// Gets type of result control
        /// </summary>
        Type ControlType { get; }
    }
}