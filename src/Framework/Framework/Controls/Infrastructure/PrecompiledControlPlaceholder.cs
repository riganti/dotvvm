using System;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Represents a control which was precompiled. This class does nothing, only serves as a marker for diagnostics.
    /// </summary>
    public sealed class PrecompiledControlPlaceholder : DotvvmControl
    {
        public Type ControlType { get; }

        public PrecompiledControlPlaceholder(Type controlType)
        {
            ControlType = controlType;
        }
    }
}
