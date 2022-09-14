using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    /// <summary> Places a /* */ comment. Will not do anything if debug is set to false </summary>
    public sealed class JsCommentNode : JsNode
    {
        public string Text { get; set; } = "";
        public JsCommentNode()
        {
        }

        public JsCommentNode(string text)
        {
            Text = text;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) =>
            visitor.VisitCommentNode(this);
    }
}
