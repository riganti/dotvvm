using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsIdentifier : JsNode
    {
        string name;
        public string Name
        {
            get { return this.name; }
            set {
                ThrowIfFrozen();
                this.name = value ?? throw new ArgumentNullException("value");
            }
        }

        public JsIdentifier(string name = "")
        {
            Name = name;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitIdentifier(this);
    }
}
