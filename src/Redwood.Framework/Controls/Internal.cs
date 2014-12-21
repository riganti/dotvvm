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
            RedwoodProperty.Register<string, Internal>("IsNamingContainer", isValueInherited: false);

    }
}
