using System;
using System.Collections.Generic;
using DotVVM.Framework.Controls.DynamicData.Annotations;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    /// <summary>
    /// Provides a list of properties for the specified entity.
    /// </summary>
    public interface IEntityPropertyListProvider
    {
        /// <summary>
        /// Gets a list of properties for the specified entity and view name.
        /// </summary>
        IEnumerable<PropertyDisplayMetadata> GetProperties(Type entityType, IViewContext viewContext);
    }
}
