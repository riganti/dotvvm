using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    [ContainsRedwoodProperties]
    public class Validation
    {

        public static RedwoodProperty EnabledProperty = RedwoodProperty.Register<bool, Validation>("Enabled", true);

        public static RedwoodProperty TargetProperty = RedwoodProperty.Register<object, Validation>("Target", true);

    }
}
