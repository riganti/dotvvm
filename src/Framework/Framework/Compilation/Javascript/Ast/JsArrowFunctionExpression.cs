using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsArrowFunctionExpression: JsBaseFunctionExpression
    {

        public JsExpression? ExpressionBody
        {
            get {
                if (Block.Body.Count == 1 &&
                    Block.Body.Single() is JsReturnStatement { Expression: var exprBody })
                    return exprBody;
                else
                    return null;
            }
            set { Block = value.NotNull().Return().AsBlock(); }
        }
        


        public JsArrowFunctionExpression(IEnumerable<JsIdentifier> parameters, JsBlockStatement bodyBlock, bool isAsync = false)
        {
            foreach (var p in parameters) AddChild(p, ParametersRole);
            AddChild(bodyBlock, BlockRole);
            IsAsync = isAsync;
        }
        public JsArrowFunctionExpression(IEnumerable<JsIdentifier> parameters, JsExpression expressionBody, bool isAsync = false)
            : this(parameters, expressionBody.Return().AsBlock(), isAsync) { }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitArrowFunctionExpression(this);

        public static JsExpression CreateIIFE(JsExpression expression, IEnumerable<(string name, JsExpression initExpression)>? parameters = null, bool isAsync = false) =>
            CreateIIFE(expression.Return().AsBlock(), parameters, isAsync);
        public static JsExpression CreateIIFE(JsBlockStatement block, IEnumerable<(string name, JsExpression initExpression)>? parameters = null, bool isAsync = false)
        {
            if (parameters == null) parameters = Enumerable.Empty<(string, JsExpression)>();
            return new JsArrowFunctionExpression(
                parameters.Select(p => new JsIdentifier(p.name)),
                block,
                isAsync
            ).Invoke(parameters.Select(p => p.initExpression));
        }
    }
}
