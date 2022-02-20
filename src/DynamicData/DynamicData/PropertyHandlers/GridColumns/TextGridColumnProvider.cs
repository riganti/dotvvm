using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns
{
    public class TextGridColumnProvider : GridColumnProviderBase
    {
        public override bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context)
        {
            return TextBoxHelper.CanHandleProperty(propertyInfo, context);
        }

        protected override GridViewColumn CreateColumnCore(GridView gridView, PropertyDisplayMetadata property, DynamicDataContext context)
        {
            var column = new GridViewTextColumn();
            column.FormatString = property.FormatString;
            column.SetBinding(GridViewTextColumn.ValueBindingProperty, context.CreateValueBinding(property));
            column.IsEditable = property.IsEditable;
            return column;
        }
    }
}
