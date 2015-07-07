using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [ContainsDotvvmProperties]
    public class RenderSettings
    {

        [AttachedProperty]
        public static readonly DotvvmProperty ModeProperty =
            DotvvmProperty.Register<RenderMode, RenderSettings>("Mode", RenderMode.Client, true);

    }
}
