using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StylingVisitor: ResolvedControlTreeVisitor
    {
        public StyleMatcher Matcher { get; set; }

        public StylingVisitor(StyleMatcher matcher)
        {
            this.Matcher = matcher;
        }

        public StylingVisitor(StyleRepository styleRepo) : this(styleRepo.CreateMatcher())
        { }

        public override void VisitControl(ResolvedControl control)
        {
            Matcher.PushControl(control);
            foreach (var style in Matcher.GetMatchingStyles())
            {
                style.ApplyStyle(control);
            }
            base.VisitControl(control);
            Matcher.PopControl();
        }
    }
}
