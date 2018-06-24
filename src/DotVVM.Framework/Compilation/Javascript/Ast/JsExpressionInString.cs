namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsExpressionInString : JsExpression
    {
        private readonly ParametrizedCode parametrizedCode;

        public string Expression => parametrizedCode.ToDefaultString();
        public byte OperatorPrecedence => parametrizedCode.OperatorPrecedence.Precedence;
        public bool IsPreferedSide => parametrizedCode.OperatorPrecedence.IsPreferedSide;

        public JsExpressionInString(ParametrizedCode parametrizedCode)
        {
            this.parametrizedCode = parametrizedCode;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitJsExpressionInString(this);
    }
}
