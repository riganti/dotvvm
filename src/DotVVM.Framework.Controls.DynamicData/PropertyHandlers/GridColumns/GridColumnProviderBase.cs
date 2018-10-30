using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns
{
    public abstract class GridColumnProviderBase : DynamicDataPropertyHandlerBase, IGridColumnProvider
    {
        public GridViewColumn CreateColumn(GridView gridView, PropertyDisplayMetadata property, DynamicDataContext context)
        {
            var column = CreateColumnCore(gridView, property, context);

            column.CssClass = ControlHelpers.ConcatCssClasses(column.CssClass, property.Styles?.GridCellCssClass);
            column.HeaderCssClass = ControlHelpers.ConcatCssClasses(column.HeaderCssClass, property.Styles?.GridHeaderCellCssClass);

            return column;
        }

        protected abstract GridViewColumn CreateColumnCore(GridView gridView, PropertyDisplayMetadata property, DynamicDataContext context);
    }
}