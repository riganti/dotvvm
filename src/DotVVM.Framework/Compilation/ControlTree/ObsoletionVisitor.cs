using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation
{
    public class ObsoletionVisitor : ResolvedControlTreeVisitor
    {
        public override void DefaultVisit(IResolvedTreeNode node)
        {
            base.DefaultVisit(node);
            if (!(node is ResolvedPropertySetter setter) || setter.Property.ObsoleteAttribute is null)
            {
                return;
            }

            if (setter.Property.ObsoleteAttribute.IsError)
            {
                setter.DothtmlNode.AddError(setter.Property.ObsoleteAttribute.Message);
            }
            else
            {
                setter.DothtmlNode.AddWarning(setter.Property.ObsoleteAttribute.Message);
            }
        }
    }
}
