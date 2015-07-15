using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public class KeywordExpression : BindingExpressionNode
    {
        public override bool IsViewModel => true;
        public KeywordType Keyword { get; set; }
        public int ParentIndex
        {
            get
            {
                if ((int)Keyword < (int)KeywordType.Parent)
                    throw new InvalidOperationException("not parent");
                return (int)Keyword - (int)KeywordType.Parent;
            }
        }

        public KeywordExpression(KeywordType keyword, Type type)
        {
            Type = type;
            Keyword = keyword;
        }

        public override void Accept(BindingExpressionTreeVisitor visitor)
        {
            visitor.VisitKeyword(this);
        }

        public override void AcceptChildred(BindingExpressionTreeVisitor visitor)
        {
        }
    }

    public enum KeywordType
    {
        This,
        Root,
        Parent
    }
}
