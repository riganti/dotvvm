using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation;

namespace DotVVM.Framework.Controls.DynamicData
{
    /// <summary>
    /// Creates the editor for the specified property using the metadata information.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, Precompile = ControlPrecompilationMode.InServerSideStyles)]
    public sealed class DynamicEditor: CompositeControl, IObjectWithCapability<HtmlCapability>, IControlWithHtmlAttributes
    {
        /// <summary>
        /// Gets or sets the property that should be edited.
        /// </summary>
        [BindingCompilationRequirements(required: new [] { typeof(ReferencedViewModelPropertiesBindingProperty) })]
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public IValueBinding? Property
        {
            get { return (IValueBinding?)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }

        public VirtualPropertyGroupDictionary<object?> Attributes =>
            new(this, AttributesGroupDescriptor);
        [MarkupOptions(MappingMode = MappingMode.Attribute, AllowBinding = true, AllowHardCodedValue = true, AllowValueMerging = true, AttributeValueMerger = typeof(HtmlAttributeValueMerger), AllowAttributeWithoutValue = true)]
        public static DotvvmPropertyGroup AttributesGroupDescriptor =
            DotvvmPropertyGroup.Register<object, DynamicEditor>(new [] { "", "html:" }, nameof(Attributes));

        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<IValueBinding, DynamicEditor>(c => c.Property, null);


        public ITemplate? OverrideTemplate
        {
            get { return (ITemplate?)GetValue(OverrideTemplateProperty); }
            set { SetValue(OverrideTemplateProperty, value); }
        }
        public static readonly DotvvmProperty OverrideTemplateProperty =
            DotvvmProperty.Register<ITemplate, DynamicEditor>(c => c.OverrideTemplate, null);

        public string[] Tags
        {
            get { return (string[]?)GetValue(TagsProperty) ?? Array.Empty<string>(); }
            set { SetValue(TagsProperty, value); }
        }

        public static readonly DotvvmProperty TagsProperty =
            DotvvmProperty.Register<string[], DynamicEditor>("Tags", Array.Empty<string>());

        private readonly IServiceProvider services;

        public DynamicEditor(IServiceProvider services)
        {
            this.services = services;
        }

        public DotvvmControl GetContents(Props props)
        {
            if (props.OverrideTemplate is {})
            {
                return new TemplateHost(props.OverrideTemplate);
            }

            var context = new DynamicDataContext(this.GetDataContextType().NotNull(), this.services)
            {
                ViewName = null,
                GroupName = null
            };

            if (Property is null)
                throw new DotvvmControlException(this, $"{nameof(Property)} is not set.");

            var prop = Property.GetProperty<ReferencedViewModelPropertiesBindingProperty>();

            if (prop.MainProperty is null)
                throw new NotSupportedException($"The binding {Property} must be bound to a single property. Alternatively, you can write a custom server-side style rule for your expression.");

            var propertyMetadata = context.PropertyDisplayMetadataProvider.GetPropertyMetadata(prop.MainProperty);

            var editorProvider =
                context.DynamicDataConfiguration.FormEditorProviders
                    .FirstOrDefault(e => e.CanHandleProperty(prop.MainProperty, context));

            if (editorProvider is null)
                throw new DotvvmControlException(this, $"Editor provider for property {prop.MainProperty} could not be found.");

            var control = editorProvider.CreateControl(propertyMetadata, props, context);
            if (props.Tags.Length > 0)
                control.SetValue(DynamicEditor.TagsProperty, props.Tags);
            return control;
        }

        [DotvvmControlCapability]
        public sealed record Props
        {
            public IValueBinding? Property { get; init; }
            public ICommandBinding? Changed { get; init; }
            public ValueOrBinding<bool> Enabled { get; init; } = new(true);
            public HtmlCapability Html { get; init; } = new();
            public ITemplate? OverrideTemplate { get; init; }
            /// <summary> Internal tags for styles to match on. </summary>
            public string[] Tags { get; init; } = new string[0];
        }
    }
}
