using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls.DynamicData.Annotations;
using Humanizer;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    public class PropertyDisplayMetadata
    {
        public PropertyInfo PropertyInfo { get; }

        public LocalizableString? DisplayName { get; set; }
        public LocalizableString? Placeholder { get; set; }
        public LocalizableString? Description { get; set; }

        public string? GroupName { get; set; }

        public int? Order { get; set; }

        public string? FormatString { get; set; }

        public string? NullDisplayText { get; set; }

        public bool AutoGenerateField { get; set; }

        public IEnumerable<VisibleAttribute> VisibleAttributes { get; set; } = Enumerable.Empty<VisibleAttribute>();

        public DataType? DataType { get; set; }

        public bool IsEditable { get; set; }

        public IEnumerable<EnabledAttribute> EnabledAttributes { get; set; } = Enumerable.Empty<EnabledAttribute>();

        public StyleAttribute? Styles { get; set; }

        public SelectorAttribute? SelectorConfiguration { get; set; }


        public bool IsDefaultLabelAllowed { get; set; }


        public PropertyDisplayMetadata(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }



        /// <summary> Returns DisplayName or a default name derived from the PropertyInfo if it is not set. </summary>
        public LocalizableString GetDisplayName() =>
            DisplayName ?? LocalizableString.Constant(PropertyInfo.Name.Humanize());
    }
}
