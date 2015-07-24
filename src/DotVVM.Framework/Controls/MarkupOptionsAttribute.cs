using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Declares instructions for control builder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MarkupOptionsAttribute : Attribute
    {

        /// <summary>
        /// Gets or sets whether the data-binding can be used on this property.
        /// </summary>
        public bool AllowBinding { get; set; }

        /// <summary>
        /// Gets or sets whether the hard-coded value in markup can be used on this property.
        /// </summary>
        public bool AllowHardCodedValue { get; set; }

        /// <summary>
        /// Gets or sets the name in markup.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the mapping mode.
        /// </summary>
        public MappingMode MappingMode { get; set; }

    }
}
