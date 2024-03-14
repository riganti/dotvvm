using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Configuration
{

    public class DotvvmConfigurationSerializationResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(System.Type type, JsonSerializerOptions options)
        {
            var info = base.GetTypeInfo(type, options);
            object? defaults = null;
            if (type == typeof(DotvvmSecurityConfiguration) ||
                type == typeof(DotvvmRuntimeConfiguration) ||
                type == typeof(DotvvmCompilationPageConfiguration) ||
                type == typeof(DotvvmPerfWarningsConfiguration) ||
                type == typeof(DotvvmDiagnosticsConfiguration) ||
                type == typeof(DotvvmMarkupConfiguration) ||
                type == typeof(DotvvmExperimentalFeaturesConfiguration) ||
                type == typeof(ViewCompilationConfiguration))
            {
                defaults = Activator.CreateInstance(type, nonPublic: true);
            }
            else if (type == typeof(DotvvmConfiguration))
            {
                defaults = new DotvvmConfiguration(new EmptyServiceProvider()) { DefaultCulture = null! };
            }
            else if (type == typeof(DotvvmPropertySerializableList.DotvvmPropertyInfo))
                defaults = new DotvvmPropertySerializableList.DotvvmPropertyInfo(null!, null, null);
            else if (type == typeof(DotvvmPropertySerializableList.DotvvmPropertyGroupInfo))
                defaults = new DotvvmPropertySerializableList.DotvvmPropertyGroupInfo(null!, null, null!, null, null);
            else if (type == typeof(DotvvmPropertySerializableList.DotvvmControlInfo))
                defaults = new DotvvmPropertySerializableList.DotvvmControlInfo(typeof(DotvvmControl).Assembly.GetName().Name, null, null, false, null, false, null);
            foreach (var property in info.Properties)
            {
                var originalCondition = property.ShouldSerialize ?? ((_, _) => true);
                if (defaults is {} && (property.Get is {} || property.AttributeProvider is PropertyInfo prop))
                {
                    var def = property.Get is {} ? property.Get(defaults!) : ((PropertyInfo)property.AttributeProvider!).GetValue(defaults);
                    if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                    {
                        property.ShouldSerialize = (obj, value) =>
                            originalCondition(obj, value) &&
                            !object.Equals(value, def) && // null
                            (value is not IEnumerable valE || def is not IEnumerable defE || !valE.Cast<object>().SequenceEqual(defE.Cast<object>()));
                    }
                    // else if (property.Name == "compiledViewsAssemblies" && info.Type == typeof(DotvvmConfiguration))
                    // {
                    //     property.ShouldSerialize = (obj, value) =>
                    //         originalCondition(obj, value) &&
                    //         (value is not IEnumerable<string> c || !new [] { "CompiledViews.dll" }.SequenceEqual(c));
                    // }
                    else
                    {
                        property.ShouldSerialize = (obj, value) =>
                            originalCondition(obj, value) &&
                            !object.Equals(def, value);
                    }
                }
                else if (
                    property.Name is "includedRoutes" or "excludedRoutes" ||
                    property.Name is "Dependencies" && typeof(IResource).IsAssignableFrom(type))
                {
                    property.ShouldSerialize = (obj, value) =>
                        originalCondition(obj, value) && !(value is IEnumerable e && !e.Cast<object>().Any());
                }

                // if (type.GetMethod("ShouldSerialize" + property.Name, 0, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, Array.Empty<ParameterModifier>()) is {} shouldSerializeMethod)
                // {
                //     property.ShouldSerialize = (obj, value) =>
                //         originalCondition(obj, value) && (bool)shouldSerializeMethod.Invoke(obj, Array.Empty<object>());
                // }
            }
            return info;
        }

        class EmptyServiceProvider : IServiceProvider
        {
            public object? GetService(Type serviceType) => null;
        }
    }
}
