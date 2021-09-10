using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AttachedPropertyAttribute : Attribute
    {
        private readonly Type propertyType;

        public AttachedPropertyAttribute(Type propertyType)
        {
            this.propertyType = propertyType;
        }
    }
}
