using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers
{
    public interface IDynamicDataPropertyHandler
    {
        bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context);
    }
}