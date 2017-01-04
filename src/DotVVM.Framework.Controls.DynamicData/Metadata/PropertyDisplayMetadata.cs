using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Annotations;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    public class PropertyDisplayMetadata
    {

        public PropertyInfo PropertyInfo { get; set; }

        public string DisplayName { get; set; }

        public string GroupName { get; set; }

        public int? Order { get; set; }

        public string FormatString { get; set; }

        public string NullDisplayText { get; set; }

        public bool AutoGenerateField { get; set; }

        public DataType? DataType { get; set; }

        public string[] ViewNames { get; set; }

        public StyleAttribute Styles { get; set; }
    }
}