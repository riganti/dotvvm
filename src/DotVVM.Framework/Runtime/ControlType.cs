using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime
{
    public class ControlType
    {

        public Type Type { get; private set; }

        public Type ControlBuilderType { get; private set; }

        public string VirtualPath { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ControlType"/> class.
        /// </summary>
        public ControlType(Type type, Type controlBuilderType = null, string virtualPath = null)
        {
            Type = type;
            ControlBuilderType = controlBuilderType;
            VirtualPath = virtualPath;
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((ControlType)obj);
        }

        protected bool Equals(ControlType other)
        {
            return Equals(Type, other.Type) && Equals(ControlBuilderType, other.ControlBuilderType);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (ControlBuilderType != null ? ControlBuilderType.GetHashCode() : 0);
            }
        }

    }
}