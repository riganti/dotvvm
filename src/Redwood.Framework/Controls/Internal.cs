using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Contains properties that are intended for internal use.
    /// </summary>
    [ContainsRedwoodProperties]
    public class Internal
    {
        
        public static readonly RedwoodProperty UniqueIDProperty =
            RedwoodProperty.Register<string, Internal>("UniqueID", isValueInherited: false);

        public static readonly RedwoodProperty IsNamingContainerProperty =
            RedwoodProperty.Register<bool, Internal>("IsNamingContainer", defaultValue: false, isValueInherited: false);

        public static readonly RedwoodProperty IsControlBindingTargetProperty =
            RedwoodProperty.Register<bool, Internal>("IsControlBindingTarget", defaultValue: false, isValueInherited: false);

        public static readonly RedwoodProperty IsSpaPageProperty =
            RedwoodProperty.Register<bool, Internal>("IsSpaPageProperty", defaultValue: false, isValueInherited: true);
    }
}
