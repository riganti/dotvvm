using System.Collections.Generic;
using System.ComponentModel;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [DotvvmControlCapability()]
    public sealed class RouteLinkCapability
    {
        [PropertyGroup("Query-")]
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, ValueOrBinding<object>> QueryParameters { get; private set; } = new Dictionary<string, ValueOrBinding<object>>();

        [PropertyGroup("Param-")]
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, ValueOrBinding<object>> Params { get; private set; } = new Dictionary<string, ValueOrBinding<object>>();

        public string RouteName { get; private set; }

        [DefaultValue(null)]
        public ValueOrBinding<string>? UrlSuffix { get; private set; }
        
    }
}
