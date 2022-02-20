using System;
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
        internal static PropertyDisplayMetadata[] GetPropertiesToDisplay(DynamicDataContext context)
        {
            var entityPropertyListProvider = context.Services.GetRequiredService<IEntityPropertyListProvider>();
            var viewContext = context.CreateViewContext();
            var properties = entityPropertyListProvider.GetProperties(context.EntityType);
            if (!string.IsNullOrEmpty(context.GroupName))
            {
                return properties.Where(p => p.GroupName == context.GroupName).ToArray();
            }
            return properties.ToArray();
        }

        /// <summary>
        /// Creates the contents of the label cell for the specified property.
        /// </summary>
        protected virtual DotvvmControl? InitializeControlLabel(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext, FieldProps props)
        {
            if (props.Label.ContainsKey(property.PropertyInfo.Name))
            {
                return new Literal(props.Label[property.PropertyInfo.Name]);
            }

            if (property.IsDefaultLabelAllowed)
            {
                return new Literal(property.DisplayName?.ToBinding(dynamicDataContext.BindingService) ?? new(property.PropertyInfo.Name));
            }
            return null;
        }

        protected virtual DynamicEditor CreateEditor(PropertyDisplayMetadata property, DynamicDataContext ddContext, FieldProps props)
        {
            return
                new DynamicEditor(ddContext.Services)
                .SetProperty(p => p.Property, ddContext.CreateValueBinding(property))
                .SetProperty("Changed", props.Changed.GetValueOrDefault(property.PropertyInfo.Name))
                .SetProperty("Enabled",
                    props.Enabled.GetValueOrDefault(property.PropertyInfo.Name,
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
                return new ValueOrBinding<bool>(false);
            }
            return ConditionalFieldBindingProvider.GetPropertyBinding(metadata.EnabledAttributes, context);
        }

        [DotvvmControlCapability]
        public sealed record FieldProps
        {
            [PropertyGroup("Changed-")]
            public IReadOnlyDictionary<string, ICommandBinding> Changed { get; init; } = new Dictionary<string, ICommandBinding>();
        
            [PropertyGroup("Enabled-")]
            public IReadOnlyDictionary<string, ValueOrBinding<bool>> Enabled { get; init; } = new Dictionary<string, ValueOrBinding<bool>>();

            [PropertyGroup("Label-")]
            public IReadOnlyDictionary<string, ValueOrBinding<string>> Label { get; init; } = new Dictionary<string, ValueOrBinding<string>>();

            [PropertyGroup("FieldTemplate-")]
            public IReadOnlyDictionary<string, ITemplate> FieldTemplate { get; init; } = new Dictionary<string, ITemplate>();

            [PropertyGroup("EditorTemplate-")]
            public IReadOnlyDictionary<string, ITemplate> EditorTemplate { get; init; } = new Dictionary<string, ITemplate>();
        }
    }
}
