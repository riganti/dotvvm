using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CompositeControlDecoratorAttribute : Attribute 
    {
        public Type DecoratorType { get; }

        public CompositeControlDecoratorAttribute(Type decoratorType)
        {
            this.DecoratorType = decoratorType;
        }

    }
}
