using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class AttachedPropertyAttribute : Attribute
    {

        public Type PropertyType { get; private set; }

        public AttachedPropertyAttribute(Type propertyType)
        {
            PropertyType = propertyType;
        }
    }
}
