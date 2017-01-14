using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class ExpressionDebugInfoAddingVisitor : ResolvedControlTreeVisitor
    {
        private readonly SymbolDocumentInfo symbolDoc;

        public ExpressionDebugInfoAddingVisitor(string fileName)
        {
            this.symbolDoc = Expression.SymbolDocument(fileName);
        }
        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            var binding = propertyBinding.Binding;
            var expression = binding.Expression;
            var startToken = binding.BindingNode.ValueNode.ValueToken;
            var startTokenIndex = binding.BindingNode.ValueNode.Tokens.FindElement(t => t == startToken);
            if (startTokenIndex.list != null && startTokenIndex.list.Count > startTokenIndex.from + 1)
            {
                var nextToken = startTokenIndex.list[startTokenIndex.from + 1];
                // t+
                //binding.DebugInfo = Expression.DebugInfo(symbolDoc,
                //    startToken.LineNumber,
                //    startToken.ColumnNumber + 1,
                //    nextToken.LineNumber,
                //    nextToken.ColumnNumber + 1);

            }
            base.VisitPropertyBinding(propertyBinding);
        }

    }
}
