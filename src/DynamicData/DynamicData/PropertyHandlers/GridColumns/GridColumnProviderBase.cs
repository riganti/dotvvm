using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns
{
    public abstract class GridColumnProviderBase : DynamicDataPropertyHandlerBase, IGridColumnProvider
    {
        public GridViewColumn CreateColumn(PropertyDisplayMetadata property, DynamicGridColumn.Props props, DynamicDataContext context)
        {
            var column = CreateColumnCore(property, props, context);

            column.CssClass = ControlHelpers.ConcatCssClasses(column.CssClass, property.Styles?.GridCellCssClass);
            column.HeaderCssClass = ControlHelpers.ConcatCssClasses(column.HeaderCssClass, property.Styles?.GridHeaderCellCssClass);

            return column;
        }

        protected abstract GridViewColumn CreateColumnCore(PropertyDisplayMetadata property, DynamicGridColumn.Props props, DynamicDataContext context);
    }
}
