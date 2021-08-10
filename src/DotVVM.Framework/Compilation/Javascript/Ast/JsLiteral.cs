using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Configuration;
using Newtonsoft.Json;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsLiteral : JsExpression
    {
        private object value;

        public object Value
        {
            get { return value; }
            set { ThrowIfFrozen(); this.value = value; }
        }

        /// <summary>
        /// Javascript (JSON) representation of the object.
        /// </summary>
        public string LiteralValue
        {
            get => JavascriptCompilationHelper.CompileConstant(Value);
            set => Value = JsonConvert.DeserializeObject(value, DefaultSerializerSettingsProvider.Instance.Settings);
        }

        public JsLiteral() { }
        public JsLiteral(object value)
        {
            this.Value = value;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitLiteral(this);
    }
}
