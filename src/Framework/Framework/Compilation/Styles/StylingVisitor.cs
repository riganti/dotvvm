using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;

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
            base.VisitControl(control);
            Matcher.PopControl();
        }
    }
}
