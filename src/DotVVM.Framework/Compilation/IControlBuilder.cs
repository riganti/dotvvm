using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
{
    public interface IControlBuilder
    {
        DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory, IServiceProvider services);
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