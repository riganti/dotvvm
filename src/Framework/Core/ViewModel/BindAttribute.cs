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

        public bool? _allowDynamicDispatch;
        /// <summary>
        /// When true, DotVVM serializer will select the JSON converter based on the runtime type, instead of deciding it ahead of time.
        /// This essentially enables serialization of properties defined derived types, but does not enable derive type deserialization.
        /// By default, dynamic dispatch is enabled for abstract types (including interfaces).
        /// </summary>
        public bool AllowDynamicDispatch { get => _allowDynamicDispatch ?? false; set => _allowDynamicDispatch = value; }

        /// <summary> See <see cref="AllowDynamicDispatch" /> </summary>
        public bool AllowsDynamicDispatch(bool defaultValue) => _allowDynamicDispatch ?? defaultValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="BindAttribute"/> class.
        /// </summary>
        public BindAttribute(Direction direction = Direction.Both)
        {
            Direction = direction;
        }
    }
}
