using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers
{
    public interface IDynamicDataPropertyHandler
    {
        bool CanHandleProperty(PropertyDisplayMetadata property, DynamicDataContext context);
    }
}
