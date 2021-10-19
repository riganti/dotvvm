using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public interface ISeleniumPageObjectVisitor : IResolvedControlTreeVisitor
    {
        void PushScope(PageObjectDefinition definition);
        PageObjectDefinition PopScope();
    }
}
