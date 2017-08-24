using System;
using System.Reflection;

namespace DotVVM.Framework.Controls.DynamicData.PropertyHandlers
{
    public abstract class DynamicDataPropertyHandlerBase : IDynamicDataPropertyHandler
    {

        public abstract bool CanHandleProperty(PropertyInfo propertyInfo, DynamicDataContext context);

        public static Type UnwrapNullableType(Type type)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            return type;
        }

    }
}