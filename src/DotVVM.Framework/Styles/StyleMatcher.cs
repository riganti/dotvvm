using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml;
using DotVVM.Framework.Parser.Dothtml.Parser;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;

namespace DotVVM.Framework.Styles
{
    public class StyleMatcher
    {
        public Stack<ResolvedControl> ControlStack { get; set; } = new Stack<ResolvedControl>();
        public Dictionary<Type, int> Ancestors { get; set; } = new Dictionary<Type, int>();

        public ResolvedControl Control { get; set; }

        public ILookup<Type, IStyle> Styles { get; set; }

        public StyleMatcher(IEnumerable<IStyle> styles)
        {
            Styles = styles.ToLookup(s => s.ControlType);
        }

        public void PushElement(ResolvedControl element)
        {
            if (Control != null)
                ControlStack.Push(Control);
            Control = element;
        }

        public void PopControl()
        {
            if (ControlStack.Any())
                Control = ControlStack.Pop();
            else Control = null;
        }

        public IEnumerable<IStyleApplicator> GetMatchingStyles()
        {
            var mi = GetMatchingInfo();
            return GetStyleCandidatesForControl().Where(s => s.Matches(mi)).Select(s => s.Applicator);
        }

        protected IEnumerable<IStyle> GetStyleCandidatesForControl()
        {
            var type = Control.Metadata.Type;
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

        public StyleMatchingInfo GetMatchingInfo()
        {
            return new StyleMatchingInfo()
            {
                ParentStack = ControlStack,
                Parents = Ancestors,
                Control = Control
            };
        }
    }
}
