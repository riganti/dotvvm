namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IDataContextStack
    {
        ITypeDescriptor DataContextType { get; } 

        IDataContextStack Parent { get; }

        NamespaceImport[] NamespaceImports { get; set; }
    }
}