using System.ComponentModel.DataAnnotations;
using System.Reflection;

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
    }
}