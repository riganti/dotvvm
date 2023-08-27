using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI.PropertyHandlers.GridColumns
{
    public class TextGridColumnProvider : GridColumnProviderBase
    {
        public override bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context)
        {
            return TextBoxHelper.CanHandleProperty(property.Type);
        }

        protected override GridViewColumn CreateColumnCore(PropertyDisplayMetadata property, AutoGridViewColumn.Props props, AutoUIContext context)
        {
            var column = new GridViewTextColumn();
            column.FormatString = property.FormatString;
            column.SetBinding(GridViewTextColumn.ValueBindingProperty, props.Property);
            column.SetProperty(c => c.IsEditable, property.IsEnabledBinding(context));
            return column;
        }
    }
}
