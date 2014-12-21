using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Controls
{
    public class Internal
    {
        
        public static readonly RedwoodProperty UniqueIDProperty =
            RedwoodProperty.Register<string, Internal>("UniqueID", isValueInherited: false);

        public static readonly RedwoodProperty IsNamingContainerProperty =
            RedwoodProperty.Register<bool, Internal>("IsNamingContainer", defaultValue: false, isValueInherited: false);

        public static readonly RedwoodProperty IsControlBindingTargetProperty =
            RedwoodProperty.Register<bool, Internal>("IsControlBindingTarget", defaultValue: false, isValueInherited: false);
    }
}
