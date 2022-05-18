using DotVVM.AutoUI.Metadata;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public abstract class DynamicDataPropertyHandlerBase : IDynamicDataPropertyHandler
    {
        public abstract bool CanHandleProperty(PropertyDisplayMetadata property, DynamicDataContext context);
    }
}
