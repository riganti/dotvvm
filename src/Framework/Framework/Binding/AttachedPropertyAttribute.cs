using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    /// <summary> Used to mark DotvvmProperty which are used on other control than the declaring type. For example, Validation.Target is an attached property. </summary>
    /// <remark> Note that DotVVM allows this for any DotvvmProperty, but this attribute instructs editor extension to include the property in autocompletion. </remark>
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
