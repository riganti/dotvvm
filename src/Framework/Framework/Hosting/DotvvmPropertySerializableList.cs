using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Hosting
{
    static class DotvvmPropertySerializableList
    {
        public static SortedDictionary<string, SortedDictionary<string, DotvvmPropertyInfo>> Properties =>
            DotvvmProperty.AllProperties
            .Where(p => p is not DotvvmCapabilityProperty)
            .Select(p =>
                new {
                    declaringType = p.DeclaringType,
                    name = p.Name,
                    p = new DotvvmPropertyInfo(
                        p.PropertyType,
                        // p.DefaultValue,
                        p.DataContextChangeAttributes.Length > 0 ? p.DataContextChangeAttributes : null,
                        p.DataContextManipulationAttribute,
                        p.IsValueInherited,
                        p.MarkupOptions.Name != p.Name ? p.MarkupOptions.Name : null,
                        p.MarkupOptions.MappingMode,
                        p.MarkupOptions.Required,
                        p is ActiveDotvvmProperty,
                        p is CompileTimeOnlyDotvvmProperty,
                        fromCapability: p.OwningCapability?.Name,
                        isAttached: p.AttributeProvider?.IsDefined(typeof(AttachedPropertyAttribute), true) == true ||
                                    p.PropertyInfo?.IsDefined(typeof(AttachedPropertyAttribute), true) == true
                    )
                }
            )
            .GroupBy(p => p.declaringType)
            .ToDictionary(p => p.Key.FullName!, p => p.ToDictionary(p => p.name, p => p.p).ToSorted(StringComparer.OrdinalIgnoreCase)).ToSorted(StringComparer.OrdinalIgnoreCase);

        public static SortedDictionary<string, SortedDictionary<string, DotvvmPropertyInfo>> Capabilities =>
            DotvvmProperty.AllProperties
            .OfType<DotvvmCapabilityProperty>()
            .Select(p =>
                new {
                    declaringType = p.DeclaringType,
                    name = p.Name,
                    p = new DotvvmPropertyInfo(
                        p.PropertyType,
                        p.DataContextChangeAttributes.Length > 0 ? p.DataContextChangeAttributes : null,
                        p.DataContextManipulationAttribute,
                        capabilityPrefix: p.Prefix,
                        fromCapability: p.OwningCapability?.Name
                    )
                }
            )
            .GroupBy(p => p.declaringType)
            .ToDictionary(p => p.Key.FullName!, p => p.ToDictionary(p => p.name, p => p.p).ToSorted(StringComparer.OrdinalIgnoreCase)).ToSorted(StringComparer.OrdinalIgnoreCase);

        public static SortedDictionary<string, SortedDictionary<string, DotvvmPropertyGroupInfo>> PropertyGroups =>
            DotvvmPropertyGroup.AllGroups
            .Select(p =>
                new {
                    declaringType = p.DeclaringType,
                    name = p.Name,
                    p = new DotvvmPropertyGroupInfo(
                        p.Prefixes.Length != 1 ? p.Prefixes : null,
                        p.Prefixes.Length == 1 ? p.Prefixes[0] : null,
                        p.PropertyType,
                        p.DataContextChangeAttributes.Length > 0 ? p.DataContextChangeAttributes : null,
                        p.DataContextManipulationAttribute,
                        p.MarkupOptions.MappingMode,
                        fromCapability: p.OwningCapability?.Name,
                        isAttached: p.AttributeProvider?.IsDefined(typeof(AttachedPropertyAttribute), true) == true
                    )
                }
            )
            .GroupBy(p => p.declaringType)
            .ToDictionary(p => p.Key.FullName!, p => p.ToDictionary(p => p.name, p => p.p).ToSorted(StringComparer.OrdinalIgnoreCase)).ToSorted(StringComparer.OrdinalIgnoreCase);

        public static SortedDictionary<string, DotvvmControlInfo> GetControls(CompiledAssemblyCache assemblies)
        {
            var result = new SortedDictionary<string, DotvvmControlInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in assemblies.GetAllAssemblies())
            {
                foreach (var c in a.GetLoadableTypes())
                {
                    if (!typeof(DotvvmBindableObject).IsAssignableFrom(c)) continue;

                    var markupOptions = c.GetCustomAttribute<ControlMarkupOptionsAttribute>();
                    var control = new DotvvmControlInfo(
                        c.Assembly.GetName().Name,
                        c.BaseType,
                        isAbstract: c.IsAbstract || c.IsGenericType,
                        markupOptions?.DefaultContentProperty,
                        !markupOptions?.AllowContent ?? false
                    );
                    result[c.FullName] = control;
                }
            }

            return result;
        }

        public record DotvvmPropertyInfo(
            Type type,
            DataContextChangeAttribute[]? dataContextChange,
            DataContextStackManipulationAttribute? dataContextManipulation,
            bool isValueInherited = false,
            string? mappingName = null,
            [property: DefaultValue(MappingMode.Attribute)]
            MappingMode mappingMode = MappingMode.Attribute,
            bool required = false,
            bool isActive = false,
            bool isCompileTimeOnly = false,
            string? fromCapability = null,
            [property: DefaultValue("")]
            string capabilityPrefix = "",
            bool isAttached = false
        ) { }

        public record DotvvmPropertyGroupInfo(
            string[]? prefixes,
            string? prefix,
            Type type,
            DataContextChangeAttribute[]? dataContextChange,
            DataContextStackManipulationAttribute? dataContextManipulation,
            [property: DefaultValue(MappingMode.Attribute)]
            MappingMode mappingMode = MappingMode.Attribute,
            string? fromCapability = null,
            bool isAttached = false
        ) { }


        public record DotvvmControlInfo(
            string assembly,
            Type baseType,
            bool isAbstract,
            string? defaultContentProperty,
            bool withoutContent
        ) { }


    }
}
