using System;
using System.Diagnostics;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation
{
    public sealed class ControlType : IControlType
    {
        public Type Type { get; private set; }

        public string? VirtualPath { get; private set; }

        public Type? DataContextRequirement { get; private set; }

        ITypeDescriptor IControlType.Type => new ResolvedTypeDescriptor(Type);

        ITypeDescriptor? IControlType.DataContextRequirement => ResolvedTypeDescriptor.Create(DataContextRequirement);

        public string PrimaryName => GetControlNames(Type).primary;

        public string[] AlternativeNames => GetControlNames(Type).alternative;

        static void ValidateControlClass(Type control)
        {
            if (!control.IsPublic && !control.IsNestedPublic)
                throw new Exception($"Control {control.FullName} is not publicly accessible. Make sure that control is not internal.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlType"/> class.
        /// </summary>
        public ControlType(Type type, string? virtualPath = null, Type? dataContextRequirement = null)
        {
            ValidateControlClass(type);
            Type = type;
            VirtualPath = virtualPath;
            if (dataContextRequirement != typeof(Binding.UnknownTypeSentinel))
                DataContextRequirement = dataContextRequirement;
        }

        public static (string primary, string[] alternative) GetControlNames(Type controlType)
        {
            var attr = controlType.GetCustomAttribute<ControlMarkupOptionsAttribute>();
            if (attr is null)
            {
                return (controlType.Name, Array.Empty<string>());
            }
            else
            {
                return (attr.PrimaryName ?? controlType.Name, attr.AlternativeNames ?? Array.Empty<string>());
            }
        }


        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is ControlType ct && Equals(ct);
        }

        public bool Equals(ControlType other)
        {
            return Equals(Type, other.Type) && VirtualPath == other.VirtualPath;
        }

        public override int GetHashCode() =>
            (Type, VirtualPath).GetHashCode();
    }
}
