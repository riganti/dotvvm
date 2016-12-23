using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns
{
    public abstract class GridColumnProviderBase : DynamicDataPropertyHandlerBase, IGridColumnProvider
    {
        public abstract GridViewColumn CreateColumn(GridView gridView, PropertyDisplayMetadata property, DynamicDataContext context);
    }
}