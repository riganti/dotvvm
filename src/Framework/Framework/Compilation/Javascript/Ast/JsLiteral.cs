using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsLiteral : JsExpression
    {
        private object? value;

        public object? Value
        {
            get { return value; }
            set { ThrowIfFrozen(); this.value = value; }
        }

        /// <summary>
        /// Javascript (JSON) representation of the object.
        /// </summary>
        public string LiteralValue
        {
            // this is a compile-time AST node, so the values should not be controlled by an adversary
            // plus, the value often contains base-64 encoded data (command IDs) which is affected by
            // System.Text.Json the HTML-safe encoder (they encode + sign)
            // However, since the value may end up in a HTML comment, we manually escape < and > to be on the safe side
            // (> is necessary to escape the comment, > just to be sure)
            get => JavascriptCompilationHelper.CompileConstant(Value, htmlSafe: false)
                                              .Replace("<", "\\u003C")
                                              .Replace(">", "\\u003E");
            set => Value = JsonDocument.Parse(value).RootElement;
        }

        public JsLiteral() { }
        public JsLiteral(object? value)
        {
            this.Value = value;
        }
        public JsLiteral(bool value): this(BoxingUtils.Box(value)) { }
        public JsLiteral(int value): this(BoxingUtils.Box(value)) { }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitLiteral(this);


        public static JsLiteral Null => new JsLiteral(null);
    }
}
