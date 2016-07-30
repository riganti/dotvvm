using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    [DebuggerDisplay("{Name}='{{Value}}'")]
    public class ResolvedHtmlAttributeValue : ResolvedHtmlAttributeSetter, IAbstractHtmlAttributeValue
    {

        public object Value { get; set; }

        public ResolvedHtmlAttributeValue(string name, object value)
            :base(name)
        {
            Value = value;
        }

        public override void Accept(IResolvedControlTreeVisitor visitor)
        {
            visitor.VisitHtmlAttributeValue(this);
        }

        public override void AcceptChildren(IResolvedControlTreeVisitor visitor)
        {
            
        }
    }
}
