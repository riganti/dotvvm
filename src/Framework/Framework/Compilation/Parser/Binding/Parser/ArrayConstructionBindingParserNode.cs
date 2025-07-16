using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ArrayConstructionBindingParserNode : BindingParserNode
    {
        public BindingParserNode? ElementTypeExpression { get; private set; }
        public BindingParserNode? SizeExpression { get; private set; }
        public List<BindingParserNode>? InitializerExpressions { get; private set; }

        // Constructor for sized array: new int[5]
        public ArrayConstructionBindingParserNode(BindingParserNode elementTypeExpression, BindingParserNode sizeExpression)
        {
            ElementTypeExpression = elementTypeExpression;
            SizeExpression = sizeExpression;
            InitializerExpressions = null;
        }

        // Constructor for array with initializers: new[] { 1, 2, 3 } or new int[] { 1, 2, 3 }
        public ArrayConstructionBindingParserNode(BindingParserNode? elementTypeExpression, List<BindingParserNode> initializerExpressions)
        {
            ElementTypeExpression = elementTypeExpression;
            SizeExpression = null;
            InitializerExpressions = initializerExpressions;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            var nodes = base.EnumerateNodes();
            
            if (ElementTypeExpression != null)
                nodes = nodes.Concat(ElementTypeExpression.EnumerateNodes());
            
            if (SizeExpression != null)
                nodes = nodes.Concat(SizeExpression.EnumerateNodes());
            
            if (InitializerExpressions != null)
                nodes = nodes.Concat(InitializerExpressions.SelectMany(e => e.EnumerateNodes()));

            return nodes;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            var nodes = new List<BindingParserNode>();
            
            if (ElementTypeExpression != null)
                nodes.Add(ElementTypeExpression);
            
            if (SizeExpression != null)
                nodes.Add(SizeExpression);
            
            if (InitializerExpressions != null)
                nodes.AddRange(InitializerExpressions);

            return nodes;
        }

        public override string ToDisplayString()
        {
            if (SizeExpression != null)
            {
                // Sized array: new int[5]
                var typeDisplay = ElementTypeExpression?.ToDisplayString() ?? "";
                return $"new {typeDisplay}[{SizeExpression.ToDisplayString()}]";
            }
            else if (InitializerExpressions != null)
            {
                // Array with initializers: new[] { 1, 2, 3 } or new int[] { 1, 2, 3 }
                var typeDisplay = ElementTypeExpression?.ToDisplayString();
                var initializers = string.Join(", ", InitializerExpressions.Select(e => e.ToDisplayString()));
                
                if (string.IsNullOrEmpty(typeDisplay))
                {
                    return $"new[] {{ {initializers} }}";
                }
                else
                {
                    return $"new {typeDisplay}[] {{ {initializers} }}";
                }
            }
            else
            {
                return "new[]";
            }
        }
    }
}
