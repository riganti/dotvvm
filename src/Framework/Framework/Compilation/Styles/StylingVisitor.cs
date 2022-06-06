using System.Collections.Generic;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StylingVisitor: ResolvedControlTreeVisitor
    {
        private DotvvmConfiguration configuration;

        public StyleMatcher Matcher { get; }

        public StylingVisitor(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            this.Matcher = configuration.Styles.CreateMatcher();
        }

        public override void VisitControl(ResolvedControl control)
        {
            Matcher.PushControl(control);
            foreach (var style in Matcher.GetMatchingStyles())
            {
                style.ApplyStyle(control, Matcher.Context.NotNull());
            }

            if (typeof(CompositeControl).IsAssignableFrom(control.Metadata.Type) &&
                control.Metadata.PrecompilationMode == ControlPrecompilationMode.InServerSideStyles &&
                !control.Properties.ContainsKey(Controls.Styles.ReplaceWithProperty) &&
                control.Properties.GetValueOrDefault(Controls.Styles.RemoveProperty).As<ResolvedPropertyValue>()?.Value is not true)
            {
                var replacement = ControlPrecompilationVisitor.Precompile(control, control.Metadata.PrecompilationMode, configuration.ServiceProvider).NotNull();

                var wrapper = new ResolvedControl(
                    (ControlResolverMetadata)configuration.ServiceProvider
                        .GetRequiredService<IControlResolver>()
                        .ResolveControl(new ResolvedTypeDescriptor(typeof(PrecompiledControlPlaceholder))),
                    control.DothtmlNode,
                    control.DataContextTypeStack
                );
                wrapper.ConstructorParameters = new object[] { control.Metadata.Type };
                wrapper.Content.AddRange(replacement);

                control.SetProperty(new ResolvedPropertyControl(Controls.Styles.ReplaceWithProperty, wrapper));
            }
            base.VisitControl(control);
            Matcher.PopControl();
        }
    }
}
