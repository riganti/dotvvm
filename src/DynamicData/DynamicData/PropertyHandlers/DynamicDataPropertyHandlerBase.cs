using System;
using System.Reflection;
using DotVVM.Framework.Controls.DynamicData.Metadata;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers
{
    public abstract class DynamicDataPropertyHandlerBase : IDynamicDataPropertyHandler
    {
        public abstract bool CanHandleProperty(PropertyDisplayMetadata property, DynamicDataContext context);
    }
}
