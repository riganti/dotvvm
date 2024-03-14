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
            get => JavascriptCompilationHelper.CompileConstant(Value, htmlSafe: false).Replace("<", "\\u003C");
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
