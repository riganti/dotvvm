#nullable enable
using DotVVM.Framework.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Declares instructions for control builder.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MarkupOptionsAttribute : Attribute
    {

        /// <summary>
        /// Gets or sets whether the data-binding can be used on this property.
        /// </summary>
        public bool AllowBinding { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the hard-coded value in markup can be used on this property.
        /// </summary>
        public bool AllowHardCodedValue { get; set; } = true;

        /// <summary>
        /// Gets or sets the name in markup. Null means that the name of the property should be used.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Determines if multiple property assignments can be merged into one value
        /// </summary>
        public bool AllowValueMerging { get; set; }

        /// <summary>
        /// Type with non parametric constructor which implements IAttributeValueMerger interface
        /// </summary>
        public Type AttributeValueMerger { get; set; } = typeof(DefaultAttributeValueMerger);

        /// <summary>
        /// Gets or sets the mapping mode.
        /// </summary>
        public MappingMode MappingMode { get; set; } = MappingMode.Attribute;

        /// <summary>
        /// Determines whether attributes without value are allowed.
        /// </summary>
        public bool AllowAttributeWithoutValue { get; set; }

        public bool Required { get; set; }
    }
}
