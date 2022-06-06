using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI.PropertyHandlers.GridColumns
{
    public interface IGridColumnProvider : IDynamicDataPropertyHandler
    {

        GridViewColumn CreateColumn(PropertyDisplayMetadata property, AutoGridViewColumn.Props props, DynamicDataContext context);

    }
}
