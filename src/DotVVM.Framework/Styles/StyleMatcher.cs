using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Parser.Dothtml;
using DotVVM.Framework.Parser.Dothtml.Parser;

namespace DotVVM.Framework.Styles
{
    public class StyleMatcher
    {
        public Stack<DothtmlElementNode> ElementStack { get; set; }
        public Dictionary<Type, int> Ancestors { get; set; }

        public void PushElement(DothtmlElementNode element)
        {
            ElementStack.Push(element);
        }
    }
}
