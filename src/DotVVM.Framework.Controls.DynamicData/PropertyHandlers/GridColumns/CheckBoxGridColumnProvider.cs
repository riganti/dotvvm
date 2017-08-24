using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns
{
    public class CheckBoxGridColumnProvider : GridColumnProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return UnwrapNullableType(propertyInfo.PropertyType) == typeof(bool);
        }

        protected override GridViewColumn CreateColumnCore(GridView gridView, PropertyDisplayMetadata property, DynamicDataContext context)
        {
            var column = new GridViewCheckBoxColumn();
            column.SetBinding(GridViewCheckBoxColumn.ValueBindingProperty, context.CreateValueBinding(property.PropertyInfo.Name));
            return column;
        }
    }
}