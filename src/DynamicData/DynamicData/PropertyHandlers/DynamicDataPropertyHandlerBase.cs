using System;
using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers
{
    public abstract class DynamicDataPropertyHandlerBase : IDynamicDataPropertyHandler
    {
        public abstract bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context);
    }
}
