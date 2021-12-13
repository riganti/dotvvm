using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Styles;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedControl : ResolvedContentNode, IAbstractControl
    {
        public Dictionary<DotvvmProperty, ResolvedPropertySetter> Properties { get; set; } = new Dictionary<DotvvmProperty, ResolvedPropertySetter>();

        public object[]? ConstructorParameters { get; set; }

        IEnumerable<IPropertyDescriptor> IAbstractControl.PropertyNames => Properties.Keys;

        public ResolvedControl(ControlResolverMetadata metadata, DothtmlNode? node, DataContextStack dataContext)
            : base(metadata, node, dataContext) { }

        public ResolvedControl(ControlResolverMetadata metadata, DothtmlNode? node, List<ResolvedControl>? content, DataContextStack dataContext)
            : base(metadata, node, content, dataContext) { }

        public bool SetProperty(ResolvedPropertySetter value, StyleOverrideOptions options, [NotNullWhen(false)] out string? error)
        {
            if (Properties.ContainsKey(value.Property) && options == StyleOverrideOptions.Ignore)
            {
                error = null;
                return true;
            }

            if (Properties.ContainsKey(value.Property) && options == StyleOverrideOptions.Prepend)
            {
                var old = Properties[value.Property];
                Properties[value.Property] = value;
                if (SetProperty(old, replace: false, out error))
                {
                    return true;
                }
                else
                {
                    // revert
                    SetProperty(old, replace: true, out _);
                    return false;
                }
            }

            return this.SetProperty(value, replace: options == StyleOverrideOptions.Overwrite, out error);
        }

        public void SetProperty(ResolvedPropertySetter value, bool replace = false)
        {
            if (!SetProperty(value, replace, out var error)) throw new Exception(error);
        }

        public bool SetProperty(ResolvedPropertySetter value, bool replace, [NotNullWhen(false)] out string? error)
        {
            error = null;
            if (!Properties.TryGetValue(value.Property, out var oldValue) || replace)
            {
                value.Parent = this;
                Properties[value.Property] = value;
            }
            else
            {
                if (!value.Property.MarkupOptions.AllowValueMerging) error = $"Property '{value.Property}' is already set and it's value can't be merged.";
                var merger = (IAttributeValueMerger)Activator.CreateInstance(value.Property.MarkupOptions.AttributeValueMerger)!;
                var mergedValue = (ResolvedPropertySetter?)merger.MergeResolvedValues(oldValue, value, out error);
                if (error is not null || mergedValue is null)
                {
                    error = $"Could not merge values using {value.Property.MarkupOptions.AttributeValueMerger.Name}: {error}";
                    return false;
                }
                mergedValue.Parent = this;
                Properties[mergedValue.Property] = mergedValue;
            }
            return true;
        }

        public bool RemoveProperty(DotvvmProperty property)
        {
            if (Properties.TryGetValue(property, out _))
            {
                Properties.Remove(property);
                return true;
            }

            return false;
        }

        public ResolvedPropertySetter? GetProperty(DotvvmProperty property)
        {
            if (property is DotvvmCapabilityProperty capability)
                return GetCapabilityProperty(capability);
            return Properties.TryGetValue(property, out var result) ? result : default;
        }

        public ResolvedPropertyCapability? GetCapabilityProperty(DotvvmCapabilityProperty capability)
        {
            if (capability.ResolvedControlGetter is {} getter)
                return getter(this);
            if (capability.PropertyMapping is not {} mapping || capability.PropertyGroupMapping is not {} groupMapping)
                throw new NotSupportedException($"Can not get capability {capability} on ResolvedControl as it does not have a property mapping.");
            var properties = new Dictionary<DotvvmProperty, ResolvedPropertySetter>();
            foreach (var (_, p) in mapping)
            {
                if (GetProperty(p) is {} propValue)
                    properties.Add(p, propValue);
            }
            if (!groupMapping.IsEmpty)
            {
                var groups = groupMapping.Select(g => g.dotvvmPropertyGroup).ToHashSet();
                foreach (var item in Properties)
                {
                    if (item.Key is GroupedDotvvmProperty pg && groups.Contains(pg.PropertyGroup))
                        properties.Add(item.Key, item.Value);
                }
            }

            if (properties.Count == 0)
                return null;
            return new ResolvedPropertyCapability(capability, properties);
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitControl(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            foreach (var prop in Properties.Values)
            {
                prop.Accept(visitor);
            }

            base.AcceptChildren(visitor);
        }


        public bool TryGetProperty(IPropertyDescriptor property, [NotNullWhen(true)] out IAbstractPropertySetter? value)
        {
            value = null;
            if (!Properties.TryGetValue((DotvvmProperty)property, out var result)) return false;
            value = result;
            return true;
        }
    }
}
