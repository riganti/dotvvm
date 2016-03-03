using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Styles
{
    public class StyleMatcher
    {
        public StyleMatchContext Context { get; set; }

        public ILookup<Type, IStyle> Styles { get; set; }

        public StyleMatcher(IEnumerable<IStyle> styles)
        {
            Styles = styles.ToLookup(s => s.ControlType);
        }

        public void PushControl(ResolvedControl control)
        {
            Context = new StyleMatchContext() { Control = control, Parent = Context };
        }

        public void PopControl()
        {
            if (Context == null) throw new InvalidOperationException("Stack is already empty");
            Context = Context.Parent;
        }

        public IEnumerable<IStyleApplicator> GetMatchingStyles()
        {
            return GetStyleCandidatesForControl().Where(s => s.Matches(Context)).Select(s => s.Applicator);
        }

        protected IEnumerable<IStyle> GetStyleCandidatesForControl()
        {
            var type = Context.Control.Metadata.Type;
            foreach (var s in Styles[type])
                yield return s;
            do
            {
                type = type.BaseType;

                foreach (var s in Styles[type])
                    if (!s.ExactTypeMatch)
                        yield return s;

            } while (type != typeof(object));
        }
    }
}
