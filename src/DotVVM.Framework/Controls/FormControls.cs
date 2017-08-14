using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class FormControls
    {
        [AttachedProperty(typeof(bool))]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, FormControls>(() => EnabledProperty, true, true);
    }
}
