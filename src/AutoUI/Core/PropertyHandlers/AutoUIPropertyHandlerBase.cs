using System.Collections;
using DotVVM.AutoUI.Metadata;

namespace DotVVM.AutoUI.PropertyHandlers
{
    public abstract class AutoUIPropertyHandlerBase : IAutoUIPropertyHandler
    {
        public virtual string[] UIHints => new string[] { };

        public abstract bool CanHandleProperty(PropertyDisplayMetadata property, AutoUIContext context);
    }
}
