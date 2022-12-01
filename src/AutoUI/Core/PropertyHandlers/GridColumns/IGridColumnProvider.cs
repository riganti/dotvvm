using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Controls;

namespace DotVVM.AutoUI.PropertyHandlers.GridColumns
{
    public interface IGridColumnProvider : IAutoUIPropertyHandler
    {

        GridViewColumn CreateColumn(PropertyDisplayMetadata property, AutoGridViewColumn.Props props, AutoUIContext context);

    }
}
