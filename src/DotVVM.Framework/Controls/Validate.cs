using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class Validate
    {

        [AttachedProperty]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, Validate>("Enabled", true);

        [AttachedProperty]
        public static DotvvmProperty TargetProperty = DotvvmProperty.Register<object, Validate>("Target", new ValueBindingExpression("_root"), true);

    }
}
