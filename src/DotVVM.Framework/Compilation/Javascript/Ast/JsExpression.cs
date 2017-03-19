using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public abstract class JsExpression: JsNode
    {
        public new JsExpression Clone() => (JsExpression)base.Clone();
    }
}
