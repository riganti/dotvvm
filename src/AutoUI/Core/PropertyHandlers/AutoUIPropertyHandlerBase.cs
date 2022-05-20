using DotVVM.AutoUI.Metadata;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public abstract class AutoUIPropertyHandlerBase : IAutoUIPropertyHandler
    {
        public abstract bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context);
    }
}
