﻿using System.Reflection;

namespace DotVVM.AutoUI.Metadata
{
    /// <summary>
    /// Provides information about how the property is displayed in the user interface.
    /// </summary>
    public interface IPropertyDisplayMetadataProvider
    {
        
        /// <summary>
        /// Get the metadata about how the property is displayed.
        /// </summary>
        PropertyDisplayMetadata GetPropertyMetadata(PropertyInfo property);
    }
}
