using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Controls.DynamicData.PropertyHandlers.FormEditors;

namespace DotVVM.Framework.Controls.DynamicData.Builders
{
    /// <summary>
    /// A base implementation for the form builder control.
    /// </summary>
    public abstract class FormBuilderBase : IFormBuilder
    {
        /// <summary>
        /// Gets the list of properties that should be displayed.
        /// </summary>
        protected virtual IEnumerable<PropertyDisplayMetadata> GetPropertiesToDisplay(DynamicDataContext dynamicDataContext, IEntityPropertyListProvider entityPropertyListProvider)
        {
            var viewContext = dynamicDataContext.CreateViewContext();
            var properties = entityPropertyListProvider.GetProperties(dynamicDataContext.EntityType, viewContext);
            if (!string.IsNullOrEmpty(dynamicDataContext.GroupName))
            {
                return properties.Where(p => p.GroupName == dynamicDataContext.GroupName);
            }
            return properties;
        }

        public abstract DotvvmControl BuildForm(DynamicDataContext dynamicDataContext, DynamicEntity.FieldProps fieldProps);

        protected virtual DynamicEditor CreateEditor(PropertyDisplayMetadata property, DynamicDataContext ddContext, DynamicEntity.FieldProps props)
        {
            return
                new DynamicEditor(ddContext.Services)
                .SetProperty(p => p.Property, ddContext.CreateValueBinding(property.PropertyInfo.Name))
                .SetProperty("Changed", props.Changed.GetValueOrDefault(property.PropertyInfo.Name))
                .SetProperty("Enabled", props.Enabled.GetValueOrDefault(property.PropertyInfo.Name, new(true)));
        }
    }
}
