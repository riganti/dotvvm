using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Base class for all controls that decorates another control (e.g. adds attributes).
    /// </summary>
    public abstract class Decorator : RedwoodBindableControl 
    {

        public virtual Decorator Clone()
        {
            var decorator = (Decorator)Activator.CreateInstance(GetType());

            foreach (var prop in Properties)
            {
                var value = prop.Value;
                if (value is BindingExpression)
                {
                    value = ((BindingExpression)value).Clone();
                }

                decorator.Properties[prop.Key] = value;
            }

            return decorator;
        }

    }
}
