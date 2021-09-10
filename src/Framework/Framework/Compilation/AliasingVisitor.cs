using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation
{
    public class AliasingVisitor : ResolvedControlTreeVisitor
    {
        public override void VisitControl(ResolvedControl control)
        {
            base.VisitControl(control);

            // create a copy so that the aliases can be removed
            var props = control.Properties.ToList();

            foreach(var pair in props)
            {
                if (pair.Key is DotvvmPropertyAlias alias)
                {
                    if (pair.Value.DothtmlNode is not null && control.TryGetProperty(alias.Aliased, out _))
                    {
                        pair.Value.DothtmlNode.AddError($"'{pair.Key.FullName}' is an alias for "
                            + $"'{alias.Aliased.FullName}'. Both cannot be set at once.");
                    }
                    else
                    {
                        pair.Value.Property = alias.Aliased;
                        control.SetProperty(pair.Value);
                        control.RemoveProperty(pair.Key);
                    }
                }
            }
        }
    }
}
