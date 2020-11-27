using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Testing.Generator
{
    public interface ISeleniumPageObjectVisitor : IResolvedControlTreeVisitor
    {
        void PushScope(PageObjectDefinition definition);
        PageObjectDefinition PopScope();
    }
}
