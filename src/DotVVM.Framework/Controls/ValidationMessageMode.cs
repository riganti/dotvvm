using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Modes of the <see cref="ValidationMessage" /> control behavior.
    /// </summary>
    public enum ValidationMessageMode
    {
        /// <summary>
        /// The control is hidden when the <see cref="P:ValidationMessage.ValidatedValue" /> is valid.
        /// </summary>
        HideWhenValid, 

        /// <summary>
        /// A <see cref="P:ValidationMessage.InvalidCssClass"/> is added to the control when the <see cref="P:ValidationMessage.ValidatedValue" /> is not valid.
        /// </summary>
        AddCssClass,

        /// <summary>
        /// The validation message is displayed when <see cref="P:ValidationMessage.ValidatedValue" /> is not valid.
        /// </summary>
        DisplayErrorMessage
    }
}