using DotVVM.AutoUI.Metadata;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public interface IDynamicDataPropertyHandler
    {
        bool CanHandleProperty(PropertyDisplayMetadata property, DynamicDataContext context);
    }
}
