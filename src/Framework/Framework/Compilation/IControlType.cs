using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation
{
    public interface IControlType
    {

        ITypeDescriptor Type { get; }
        
        string? VirtualPath { get; }

        ITypeDescriptor? DataContextRequirement { get; }

        string PrimaryName { get; }

        string[] AlternativeNames { get; }

    }
}
