using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.ControlTree
{
    [ContainsDotvvmProperties]
    public class LifecycleRequirementsAssigningVisitor : ResolvedControlTreeVisitor
    {
        public static readonly DotvvmProperty CompileTimeLifecycleRequirementsProperty =
            CompileTimeOnlyDotvvmProperty.Register<ControlLifecycleRequirements, LifecycleRequirementsAssigningVisitor>("CompileTimeLifecycleRequirements");
        private static readonly ConcurrentDictionary<Type, ControlLifecycleRequirements> requirementsCache = new ConcurrentDictionary<Type, ControlLifecycleRequirements>();
        public static ControlLifecycleRequirements GetRequirements(Type controlType) =>
            requirementsCache.GetOrAdd(controlType, c =>
                c.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(s => s.DeclaringType != typeof(DotvvmControl) && s.IsVirtual && s.GetBaseDefinition() != null)
                .Select(s => s.Name == "OnPreInit" ? ControlLifecycleRequirements.PreInit :
                             s.Name == "OnInit" ? ControlLifecycleRequirements.Init :
                             s.Name == "OnLoad" ? ControlLifecycleRequirements.Load :
                             s.Name == "OnPreRender" ? ControlLifecycleRequirements.PreRender :
                             s.Name == "OnPreRenderComplete" ? ControlLifecycleRequirements.PreRenderComplete :
                             ControlLifecycleRequirements.None)
                .Aggregate(ControlLifecycleRequirements.None, (a, b) => a | b)
            );

        public override void VisitControl(ResolvedControl control)
        {
            base.VisitControl(control);

            if (typeof(DotvvmControl).IsAssignableFrom(control.Metadata.Type))
            {
                var req = GetRequirements(control.Metadata.Type);
                var childReq = control.Content
                               .Select(c => c.GetValue(CompileTimeLifecycleRequirementsProperty).As<ResolvedPropertyValue>()?.Value as ControlLifecycleRequirements? ?? ControlLifecycleRequirements.None)
                               .Aggregate(ControlLifecycleRequirements.None, (a, b) => a | b);
                var value = req | childReq;
                // don't have to do the assignment for RawLiteral, as it has None by default
                if (!(value == ControlLifecycleRequirements.None && control.Metadata.Type == typeof(RawLiteral)) &&
                    // don't assign for markup controls, they already contain content when created
                    control.Metadata.VirtualPath == null)
                    control.SetProperty(new ResolvedPropertyValue(CompileTimeLifecycleRequirementsProperty, value));
            }
        }
    }
}
