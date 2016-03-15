using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class Validation
    {

        [AttachedProperty(typeof(bool))]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, Validation>(() => EnabledProperty, true, true);

        [AttachedProperty(typeof(object))]
        public static DotvvmProperty TargetProperty = DotvvmProperty.Register<object, Validation>(() => TargetProperty, null, true);

    }
}
