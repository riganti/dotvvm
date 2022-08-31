using DotVVM.AutoUI.Metadata;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public interface IAutoUIPropertyHandler
    {
        string[] UIHints { get; }

        bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context);
    }
}
