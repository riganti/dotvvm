using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class PropertyPathExtractingVisitor : JsNodeVisitor
    {
        private string fullPropertyPath;
        private string koUnwrapPathSegment;

        public string ExtractedPropertyPath { get; set; }

        public override void VisitMemberAccessExpression(JsMemberAccessExpression memberAccessExpression)
        {
            if (fullPropertyPath == null)
            {
                fullPropertyPath = memberAccessExpression.ToString();
            }
            else if (memberAccessExpression.ToString() == "ko.unwrap")
            {
                koUnwrapPathSegment = memberAccessExpression.Parent.ToString();
                ExtractPropertyPath();
            }
            base.VisitMemberAccessExpression(memberAccessExpression);
        }

        private void ExtractPropertyPath()
        {
            ExtractedPropertyPath = fullPropertyPath.Substring(koUnwrapPathSegment.Length + 1);
        }
    }
}