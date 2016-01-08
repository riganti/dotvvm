namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IDataContextStack
    {
        ITypeDescriptor DataContextType { get; } 

        IDataContextStack Parent { get; } 
    }
}