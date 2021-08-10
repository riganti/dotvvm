using System;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation
{
    public class ControlType : IControlType
    {
        public Type Type { get; private set; }

        public string VirtualPath { get; private set; }

        public Type DataContextRequirement { get; private set; }

        ITypeDescriptor IControlType.Type => new ResolvedTypeDescriptor(Type);

        ITypeDescriptor IControlType.DataContextRequirement => DataContextRequirement != null ? new ResolvedTypeDescriptor(DataContextRequirement) : null;

        protected static void ValidateControlClass(Type control)
        {
            if (!control.IsPublic)
                throw new Exception($"Control {control.FullName} is not publicly accessible. Make sure that control is not internal.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlType"/> class.
        /// </summary>
        public ControlType(Type type, string virtualPath = null, Type dataContextRequirement = null)
        {
            ValidateControlClass(type);
            Type = type;
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
            return Equals(Type, other.Type) && VirtualPath == other.VirtualPath;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 53515466) * 397) ^ (VirtualPath != null ? VirtualPath.GetHashCode() : 145132);
            }
        }
    }
}
