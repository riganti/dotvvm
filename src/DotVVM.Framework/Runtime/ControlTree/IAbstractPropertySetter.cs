namespace DotVVM.Framework.Runtime.ControlTree
{
    public interface IAbstractPropertySetter : IAbstractTreeNode
    {
        IPropertyDescriptor Property { get; }
        
    }
}