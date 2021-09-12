using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    internal class PropertyPathExtractingVisitor : JsNodeVisitor
    {
        private Stack<string> stack = new Stack<string>();

        public override void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression)
        {
            stack.Push(memberAccessExpression.MemberName);
            base.VisitMemberAccessExpression(memberAccessExpression);
        }

        public override void VisitIndexerExpression(JsIndexerExpression jsIndexerExpression)
        {
            stack.Push(jsIndexerExpression.Argument.ToString());
            base.VisitIndexerExpression(jsIndexerExpression);
        }

        public string GetPropertyPath()
        {
            var sb = new StringBuilder();
            while (stack.Count > 0)
            {
                if (sb.Length > 0)
                    sb.Append($"/{stack.Pop()}");
                else
                    sb.Append(stack.Pop());
            }

            return (sb.Length > 0) ? sb.ToString() : "/";
        }
    }
}
