using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.Controls
{
    public class PostBackHandlerCommandTypes : PostBackHandler
    {
        protected override string ClientHandlerName => nameof(PostBackHandlerCommandTypes);
        protected override Dictionary<string, object> GetHandlerOptions() => new Dictionary<string, object>();
    }
}
