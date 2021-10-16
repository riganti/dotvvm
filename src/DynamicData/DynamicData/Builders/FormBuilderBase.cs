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
            var viewContext = dynamicDataContext.CreateViewContext(dynamicDataContext.RequestContext);
            var properties = entityPropertyListProvider.GetProperties(dynamicDataContext.EntityType, viewContext);
            if (!string.IsNullOrEmpty(dynamicDataContext.GroupName))
            {
                return properties.Where(p => p.GroupName == dynamicDataContext.GroupName);
            }
            return properties;
        }

        /// <summary>
        /// Finds the editor provider for the specified property.
        /// </summary>
        protected virtual IFormEditorProvider FindEditorProvider(PropertyDisplayMetadata property, DynamicDataContext dynamicDataContext)
        {
            return dynamicDataContext.DynamicDataConfiguration.FormEditorProviders
                .FirstOrDefault(e => e.CanHandleProperty(property.PropertyInfo, dynamicDataContext));
        }

        public abstract void BuildForm(DotvvmControl hostControl, DynamicDataContext dynamicDataContext);
    }
}