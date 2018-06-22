namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsExpressionInString : JsExpression
    {
        public string Expression { get; set; }

        public JsExpressionInString(string expressions)
        {
            this.Expression = expressions;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitJsExpressionInString(this);
    }
}
