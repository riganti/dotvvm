using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyPropertyAttribute : Attribute
    {

        /// <summary>
        /// Gets the name of the property that represents the primary key.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPropertyAttribute"/> class.
        /// </summary>
        public KeyPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
