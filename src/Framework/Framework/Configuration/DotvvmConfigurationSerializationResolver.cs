using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotVVM.Framework.Configuration
{

    public class DotvvmConfigurationSerializationResolver : DefaultContractResolver
    {
        public DotvvmConfigurationSerializationResolver()
        {
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                .OrderBy(t => t.PropertyName, StringComparer.InvariantCulture)
                .ToList();
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = member as PropertyInfo;
            var property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(DotvvmExperimentalFeatureFlag) && prop is object)
            {
                // ignore defaults for brevity
                property.ShouldSerialize = o =>
                    !(prop.GetValue(o) is DotvvmExperimentalFeatureFlag flag) || flag.IsEnabledForAnyRoute();
            }
            if (property.PropertyType == typeof(DotvvmGlobalExperimentalFeatureFlag) && prop is object)
            {
                // ignore defaults for brevity
                property.ShouldSerialize = o =>
                    !(prop.GetValue(o) is DotvvmGlobalExperimentalFeatureFlag flag) || flag.Enabled;
            }

            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string) && prop is object)
            {
                property.TypeNameHandling = TypeNameHandling.None;
                var originalCondition = property.ShouldSerialize;
                property.ShouldSerialize = o =>
                    originalCondition?.Invoke(o) != false &&
                    (!(prop.GetValue(o) is IEnumerable c) || c.Cast<object>().Any());
            }

            if (prop is object && prop.Name == "CompiledViewsAssemblies" && prop.DeclaringType == typeof(DotvvmConfiguration))
            {
                property.ShouldSerialize = o =>
                    !(prop.GetValue(o) is IEnumerable<string> c) || !new [] { "CompiledViews.dll" }.SequenceEqual(c);
            }

            return property;
        }
    }
}
