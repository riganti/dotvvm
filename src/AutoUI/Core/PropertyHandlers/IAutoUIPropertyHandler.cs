using DotVVM.AutoUI.Metadata;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public interface IAutoUIPropertyHandler
    {
        bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context);
    }
}
