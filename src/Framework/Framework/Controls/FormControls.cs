using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public sealed class FormControls
    {
        /// <summary> Enables or disables all child form controls with an Enabled property (Buttons, TextBoxes, ...) </summary>
        [AttachedProperty(typeof(bool))]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, FormControls>(() => EnabledProperty, true, isValueInherited: true);

        private FormControls() {} // the class can't be static, but no instance should exist
    }
}
