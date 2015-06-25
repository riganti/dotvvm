using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    [ContainsRedwoodProperties]
    public class PostBack
    {
        [AttachedProperty]
        public static readonly RedwoodProperty UpdateProperty =
            RedwoodProperty.Register<bool, PostBack>("Update", false);
    }
}
