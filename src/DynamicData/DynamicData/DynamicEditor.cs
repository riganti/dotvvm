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

namespace DotVVM.Framework.Controls.DynamicData
{
    /// <summary>
    /// Creates the editor for the specified property using the metadata information.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, Precompile = ControlPrecompilationMode.Always)]
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

        public VirtualPropertyGroupDictionary<object?> Attributes => throw new NotImplementedException();

        public static readonly DotvvmProperty PropertyProperty
            = DotvvmProperty.Register<IValueBinding, DynamicEditor>(c => c.Property, null);

        private readonly IServiceProvider services;

        public DynamicEditor(IServiceProvider services)
        {
            this.services = services;
        }

        public DotvvmControl GetContents(HtmlCapability html)
        {
            var context = new DynamicDataContext(this.GetDataContextType().NotNull(), this.services)
            {
                ViewName = null,
                GroupName = null
            };

            var prop = Property.GetProperty<ReferencedViewModelPropertiesBindingProperty>();

            if (prop.MainProperty is null)
                throw new NotSupportedException($"The binding {Property} must be bound to a single property. Alternatively, you can write a custom server-side style rule for your expression.");

            var propertyMetadata = context.PropertyDisplayMetadataProvider.GetPropertyMetadata(prop.MainProperty);

            var editorProvider =
                context.DynamicDataConfiguration.FormEditorProviders
                    .FirstOrDefault(e => e.CanHandleProperty(prop.MainProperty, context));

            var control = editorProvider.CreateControl(propertyMetadata, context);
            if (!html.IsEmpty())
                control.SetCapability(html);
            return control;
        }
    }
}
