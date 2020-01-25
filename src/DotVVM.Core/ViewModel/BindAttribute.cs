using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// Specifies the binding direction.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BindAttribute : Attribute
    {

        /// <summary>
        /// Gets the binding direction.
        /// </summary>
        public Direction Direction { get; private set; }

        /// <summary>
        /// Name of the property in JSON and JS viewModel. Null leaves the name unmodified
        /// </summary>
        public string? Name { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="BindAttribute"/> class.
        /// </summary>
        public BindAttribute(Direction direction = Direction.Both)
        {
            Direction = direction;
        }
    }
}
