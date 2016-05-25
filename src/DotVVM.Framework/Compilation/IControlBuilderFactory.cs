namespace DotVVM.Framework.Compilation
{
    public interface IControlBuilderFactory
    {
        
        IControlBuilder GetControlBuilder(string virtualPath);

    }
}