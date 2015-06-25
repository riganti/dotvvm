using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    [ContainsRedwoodProperties]
    public class RenderSettings
    {

        [AttachedProperty]
        public static readonly RedwoodProperty ModeProperty =
            RedwoodProperty.Register<RenderMode, RenderSettings>("Mode", RenderMode.Client, true);

    }
}
