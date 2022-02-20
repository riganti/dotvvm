﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls.DynamicData
{
    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.Always)]
    public abstract class DynamicEntityBase : CompositeControl
    {
        protected readonly IServiceProvider services;
        public DynamicEntityBase(IServiceProvider services)
        {
            this.services = services;
        }


        /// <summary>
        /// Gets or sets the view name (e.g. Insert, Edit, ReadOnly). Some fields may have different metadata for each view.
        /// </summary>
        public string? ViewName
        {
            get { return (string?)GetValue(ViewNameProperty); }
            set { SetValue(ViewNameProperty, value); }
        }
        public static readonly DotvvmProperty ViewNameProperty
            = DotvvmProperty.Register<string, DynamicEntityBase>(c => c.ViewName, null);


        /// <summary>
        /// Gets or sets the group of fields that should be rendered. If not set, fields from all groups will be rendered.
        /// </summary>
        public string? GroupName
        {
            get { return (string?)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public static readonly DotvvmProperty GroupNameProperty
            = DotvvmProperty.Register<string, DynamicEntityBase>(c => c.GroupName, null);

        public static readonly DotvvmCapabilityProperty FieldPropsProperty =
            DotvvmCapabilityProperty.RegisterCapability<FieldProps, DynamicEntityBase>();

        protected DynamicDataContext CreateDynamicDataContext()
        {
            return new DynamicDataContext(this.GetDataContextType().NotNull(), this.services)
            {
                ViewName = ViewName,
                GroupName = GroupName
            };
        }

        /// <summary>
        /// Gets the list of properties that should be displayed.
        /// </summary>
        internal static PropertyDisplayMetadata[] GetPropertiesToDisplay(DynamicDataContext context, FieldSelectorProps props)
        {
            var entityPropertyListProvider = context.Services.GetRequiredService<IEntityPropertyListProvider>();
            var viewContext = context.CreateViewContext();
            var properties = entityPropertyListProvider.GetProperties(context.EntityType);

            if (props.IncludeProperties is { Length: > 0})
            {
                if (props.ExcludeProperties is { Length: > 0})
                    throw new NotSupportedException("Only one of IncludeProperties and ExcludeProperties can be specified.");

                return props.IncludeProperties.Select(prop =>
                    properties.FirstOrDefault(p => p.PropertyInfo.Name == prop)
                        ?? throw new Exception($"Property {prop} not found on entity type {context.EntityType}.")
                ).ToArray();
            }

            if (props.ExcludeProperties is {})
            {
                properties = properties.Where(p => !props.ExcludeProperties.Contains(p.PropertyInfo.Name)).ToArray();
            }

            if (!string.IsNullOrEmpty(context.GroupName))
            {
                return properties.Where(p => p.GroupName == context.GroupName).ToArray();
            }
            return properties.ToArray();
        }

        protected virtual string GetEditorId(PropertyDisplayMetadata property) => property.PropertyInfo.Name + ".input";

        /// <summary>
        /// Creates the contents of the label cell for the specified property.
        /// </summary>
        protected virtual DotvvmControl? InitializeControlLabel(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, FieldProps props)
        {
            var id = GetEditorId(property);
            if (props.Label.ContainsKey(property.PropertyInfo.Name))
            {
                return new Label(id).AppendChildren(new Literal(props.Label[property.PropertyInfo.Name]));
            }

            if (property.IsDefaultLabelAllowed)
            {
                return new Label(id).AppendChildren(new Literal(property.DisplayName?.ToBinding(dynamicDataContext) ?? new(property.PropertyInfo.Name)));
            }
            return null;
        }

        protected virtual DynamicEditor CreateEditor(PropertyDisplayMetadata property, DynamicDataContext ddContext, FieldProps props)
        {
            var name = property.PropertyInfo.Name;
            return
                new DynamicEditor(ddContext.Services)
                .SetProperty(p => p.ID, GetEditorId(property))
                .SetProperty(p => p.Property, ddContext.CreateValueBinding(property))
                .SetProperty("OverrideTemplate", props.EditorTemplate.GetValueOrDefault(name))
                .SetProperty("Changed", props.Changed.GetValueOrDefault(name))
                .SetProperty("Enabled",
                    props.Enabled.GetValueOrDefault(name,
                            GetEnabledResourceBinding(property, ddContext)));
        }

        protected virtual void InitializeValidation(HtmlGenericControl validatedElement, HtmlGenericControl labelElement, PropertyDisplayMetadata property, DynamicDataContext context)
        {
            if (context.ValidationMetadataProvider.GetAttributesForProperty(property.PropertyInfo).OfType<RequiredAttribute>().Any())
            {
                labelElement.AddCssClass("dynamicdata-required");
            }

            validatedElement.SetValue(Validator.ValueProperty, context.CreateValueBinding(property));
        }

        protected virtual ValueOrBinding<bool> GetVisibleResourceBinding(PropertyDisplayMetadata metadata, DynamicDataContext context)
        {
            return ConditionalFieldBindingProvider.GetPropertyBinding(metadata.VisibleAttributes, context);
        }

        protected virtual ValueOrBinding<bool> GetEnabledResourceBinding(PropertyDisplayMetadata metadata, DynamicDataContext context)
        {
            if (!metadata.IsEditable)
            {
                return new(false);
            }
            return ConditionalFieldBindingProvider.GetPropertyBinding(metadata.EnabledAttributes, context);
        }

        [DotvvmControlCapability]
        public sealed record FieldSelectorProps
        {
            /// <summary> Only the specified properties will be included in this form. Using ViewName, GroupName or ExcludedProperties at the same time as IncludedProperties does not make sense. </summary>
            public string[]? IncludeProperties { get; init; }
            /// <summary> The specified properties will not be included in this form. </summary>
            public string[] ExcludeProperties { get; init; } = new string[0];
            /// <summary> Gets or sets the view name (e.g. Insert, Edit, ReadOnly). Some fields may have different metadata for each view. </summary>
            public string? ViewName { get; init; }


            /// <summary> Gets or sets the group of fields that should be rendered. If not set, fields from all groups will be rendered. </summary>
            public string? GroupName { get; init; }
        }

        [DotvvmControlCapability]
        public sealed record FieldProps
        {
            /// <summary> Calls the command when the user makes changes to the specified field. For example `Changed-CountryId="{staticCommand: _root.States.Items = statesDataProvider.GetSelectorItems(_root.Address).Result}"` will reload the list of states whenever CountryId is changed. </summary>
            [PropertyGroup("Changed-")]
            public IReadOnlyDictionary<string, ICommandBinding> Changed { get; init; } = new Dictionary<string, ICommandBinding>();
        
            /// <summary> Controls if the specified property is editable. </summary>
            [PropertyGroup("Enabled-")]
            public IReadOnlyDictionary<string, ValueOrBinding<bool>> Enabled { get; init; } = new Dictionary<string, ValueOrBinding<bool>>();

            /// <summary> Overrides which text is used as a field label. </summary>
            [PropertyGroup("Label-")]
            public IReadOnlyDictionary<string, ValueOrBinding<string>> Label { get; init; } = new Dictionary<string, ValueOrBinding<string>>();

            /// <summary> Overrides how the entire form field (editor, label, ...) looks like. </summary>

            [PropertyGroup("FieldTemplate-")]
            [MarkupOptions(MappingMode = MappingMode.InnerElement)]
            public IReadOnlyDictionary<string, ITemplate> FieldTemplate { get; init; } = new Dictionary<string, ITemplate>();


            /// <summary> Overrides which component is used as an editor. </summary>
            [PropertyGroup("EditorTemplate-")]
            [MarkupOptions(MappingMode = MappingMode.InnerElement)]
            public IReadOnlyDictionary<string, ITemplate> EditorTemplate { get; init; } = new Dictionary<string, ITemplate>();


            public FieldSelectorProps FieldSelector { get; init; } = new FieldSelectorProps();

        }
    }
}
