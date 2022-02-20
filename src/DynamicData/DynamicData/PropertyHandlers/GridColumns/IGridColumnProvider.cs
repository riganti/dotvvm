using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers.GridColumns
{
    public interface IGridColumnProvider : IDynamicDataPropertyHandler
    {

        GridViewColumn CreateColumn(PropertyDisplayMetadata property, DynamicGridColumn.Props props, DynamicDataContext context);

    }
}
