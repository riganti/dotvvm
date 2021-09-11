using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation
{
    public class ObsoletionVisitor : ResolvedControlTreeVisitor
    {
        public override void DefaultVisit(IResolvedTreeNode node)
        {
            base.DefaultVisit(node);
            if (node is not ResolvedPropertySetter setter
                || setter.Property.ObsoleteAttribute is null
                || setter.DothtmlNode is null)
            {
                return;
            }

            // NB: the obsolete attribute should NEVER cause a compilation error in dothtml if the property is an alias
            if (setter.Property.ObsoleteAttribute.IsError && setter.Property is not DotvvmPropertyAlias)
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
