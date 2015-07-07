using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class PostBack
    {
        [AttachedProperty]
        public static readonly DotvvmProperty UpdateProperty =
            DotvvmProperty.Register<bool, PostBack>("Update", false);
    }
}
