using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    [ContainsRedwoodProperties]
    public class Validate
    {

        [AttachedProperty]
        public static RedwoodProperty EnabledProperty = RedwoodProperty.Register<bool, Validate>("Enabled", true);

        [AttachedProperty]
        public static RedwoodProperty TargetProperty = RedwoodProperty.Register<object, Validate>("Target", new ValueBindingExpression("_root"));

    }
}
