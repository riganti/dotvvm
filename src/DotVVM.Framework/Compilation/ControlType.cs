using System;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation
{
    public class ControlType : IControlType
    {
        public Type Type { get; private set; }

        public Type ControlBuilderType { get; private set; }

        public string VirtualPath { get; private set; }

        public Type DataContextRequirement { get; private set; }

        ITypeDescriptor IControlType.Type => new ResolvedTypeDescriptor(Type);
        
        ITypeDescriptor IControlType.DataContextRequirement => DataContextRequirement != null ? new ResolvedTypeDescriptor(DataContextRequirement) : null;


        /// <summary>
        /// Initializes a new instance of the <see cref="ControlType"/> class.
        /// </summary>
        public ControlType(Type type, Type controlBuilderType = null, string virtualPath = null, Type dataContextRequirement = null)
        {
            Type = type;
            ControlBuilderType = controlBuilderType;
            VirtualPath = virtualPath;
            DataContextRequirement = dataContextRequirement;
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