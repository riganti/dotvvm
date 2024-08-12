using System;

namespace DotVVM.Framework.Controls
{
    [Flags]
    public enum ValidatorPlacement
    {
        /// <summary> No validators are placed (automatically). </summary>
        None = 0,
        /// <summary> Validator.Value is attached to the primary editor control (i.e. a TextBox in GridViewTextColumn) </summary>
        AttachToControl = 1,
        /// <summary> A standalone Validator (span) control is placed after the editor control. </summary>
        Standalone = 2
    }
}
