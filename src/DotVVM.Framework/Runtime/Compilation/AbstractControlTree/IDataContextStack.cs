namespace DotVVM.Framework.Runtime.Compilation.AbstractControlTree
{
    public interface IDataContextStack
    {
        ITypeDescriptor DataContextType { get; } 

        IDataContextStack Parent { get; } 
    }
}