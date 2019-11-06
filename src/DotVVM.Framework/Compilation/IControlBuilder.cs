#nullable enable
using System;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
{
    public interface IControlBuilder
    {
        /// <summary>
        /// Gets required data context for the control
        /// </summary>
        Type DataContextType { get; }
        /// <summary>
        /// Gets type of result control
        /// </summary>
        Type ControlType { get; }
        DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory, IServiceProvider services);
    }

    public class ControlBuilderDescriptor
    {

        /// <summary>
        /// Gets required data context for the control
        /// </summary>
        public Type DataContextType { get; }
        /// <summary>
        /// Gets type of result control
        /// </summary>
        public Type ControlType { get; }

        public ControlBuilderDescriptor(
            Type dataContextType,
            Type controlType)
        {
            this.DataContextType = dataContextType;
            this.ControlType = controlType;
        }
    }
}
