using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    [ContainsRedwoodProperties]
    public class Validate
    {

        [AttachedProperty(typeof(bool))]
        public static RedwoodProperty EnabledProperty = RedwoodProperty.Register<bool, Validate>("Enabled", true, true);

        [AttachedProperty(typeof(object))]
        public static RedwoodProperty TargetProperty = RedwoodProperty.Register<object, Validate>("Target", null, true);

    }
}
