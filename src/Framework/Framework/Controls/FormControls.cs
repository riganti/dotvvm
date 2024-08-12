using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public sealed class FormControls
    {
        /// <summary> Enables or disables all child form controls (Buttons, TextBoxes, ...) </summary>
        [AttachedProperty(typeof(bool))]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, FormControls>(() => EnabledProperty, true, isValueInherited: true);

        /// <summary> Sets `form` attribute on all child input controls (Buttons, TextBoxes, ...), which associates them with a given form element by id. See <see href="https://developer.mozilla.org/en-US/docs/Web/HTML/Element/input#form"/> </summary>
        // [AttachedProperty(typeof(string))]
        // public static DotvvmProperty FormIDProperty = DotvvmProperty.Register<string, FormControls>(() => FormIDProperty, null, isValueInherited: true);
        private FormControls() {} // the class can't be static, but no instance should exist
    }
}
