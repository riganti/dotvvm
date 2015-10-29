using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class Validate
    {
        [AttachedProperty(typeof(bool))]
        public static DotvvmProperty EnabledProperty = DotvvmProperty.Register<bool, Validate>("Enabled", true, true);

        [AttachedProperty(typeof(object))]
        public static DotvvmProperty TargetProperty = DotvvmProperty.Register<object, Validate>("Target", null, true);

        [AttachedProperty(typeof(string[]))]
        public static DotvvmProperty GroupsProperty = DotvvmProperty.Register<string[], Validate>("Groups", null, true);
    }
     
}
