using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Testing.SeleniumGenerator
{
    public interface ISeleniumPageObjectVisitor : IResolvedControlTreeVisitor
    {
        void PushScope(PageObjectDefinition definition);
        PageObjectDefinition PopScope();
    }
}