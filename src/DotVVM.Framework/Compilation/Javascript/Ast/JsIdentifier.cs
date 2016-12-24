using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsIdentifier : JsNode
    {
        string name;
        public string Name
        {
            get { return this.name; }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");
                ThrowIfFrozen();
                this.name = value;
            }
        }

        public JsIdentifier(string name = "")
        {
            Name = name;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitIdentifier(this);
    }
}
