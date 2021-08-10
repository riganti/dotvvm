#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public sealed class FormControls
    {
        [AttachedProperty(typeof(bool))]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, FormControls>(() => EnabledProperty, true, true);
        private FormControls() {} // the class can't be static, but no instance should exist
    }
}
