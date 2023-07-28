using System.Collections.Generic;
using System.ComponentModel;
using DotVVM.Framework.Binding;

namespace DotVVM.Framework.Controls
{
    [DotvvmControlCapability()]
    public sealed record RouteLinkCapability
    {
        [PropertyGroup("Query-")]
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, ValueOrBinding<object>> QueryParameters { get; init; } = new Dictionary<string, ValueOrBinding<object>>();

        [PropertyGroup("Param-")]
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, ValueOrBinding<object>> Params { get; init; } = new Dictionary<string, ValueOrBinding<object>>();

        public string RouteName { get; init; } = null!;

        [DefaultValue(null)]
        public ValueOrBinding<string>? UrlSuffix { get; init; }
    }
}
