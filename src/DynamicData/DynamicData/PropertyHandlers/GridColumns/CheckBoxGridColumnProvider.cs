using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns
{
    public class CheckBoxGridColumnProvider : GridColumnProviderBase
    {
        public override bool CanHandleProperty(PropertyDisplayMetadata property, DynamicDataContext context)
        {
            return property.Type.UnwrapNullableType() == typeof(bool);
        }

        protected override GridViewColumn CreateColumnCore(PropertyDisplayMetadata property, DynamicGridColumn.Props props, DynamicDataContext context)
        {
            var column = new GridViewCheckBoxColumn();
            column.SetBinding(GridViewCheckBoxColumn.ValueBindingProperty, context.CreateValueBinding(property));
            return column;
        }
    }
}
