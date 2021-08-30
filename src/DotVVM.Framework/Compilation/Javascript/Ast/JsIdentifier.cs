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

        public bool IsValidName()
        {
            if (Name.Length == 0 || char.IsDigit(Name[0])) return false;
            foreach(var n in Name)
            {
                if (!char.IsLetterOrDigit(n) && n != '_' && n != '$')
                    return false;
            }
            return true;
        }

        public JsIdentifier(string? name = "")
        {
            this.name = name ?? "";
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitIdentifier(this);
    }
}
