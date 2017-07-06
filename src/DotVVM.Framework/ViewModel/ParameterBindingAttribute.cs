using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ParameterBindingAttribute : Attribute
    {

        public bool TryGetValue<T>(IDotvvmRequestContext context, out T value)
        {
            if (TryGetValueCore(context, out var valueUnconverted))
            {
                try
                {
                    value = (T) ConvertValue(valueUnconverted, typeof(T));
                    return true;
                }
                catch
                {
                    // ignore parameter binding conversion errors
                }
            }

            value = default(T);
            return false;
        }

        protected virtual object ConvertValue(object valueUnconverted, Type targetType)
        {
            return ReflectionUtils.ConvertValue(valueUnconverted, targetType);
        }

        protected abstract bool TryGetValueCore(IDotvvmRequestContext context, out object value);

    }
}
