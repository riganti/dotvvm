#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LambdaBindingParserNode : BindingParserNode
    {
        public List<LambdaParameterBindingParserNode> ParameterExpressions { get; private set; }
        public BindingParserNode BodyExpression { get; private set; }

        public LambdaBindingParserNode(List<LambdaParameterBindingParserNode> parameters, BindingParserNode body)
        {
            this.ParameterExpressions = parameters;
            this.BodyExpression = body;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat(ParameterExpressions.SelectMany(param => param.EnumerateNodes()).Concat(BodyExpression.EnumerateNodes()));

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => ParameterExpressions.Concat(new[] { BodyExpression });

        public override string ToDisplayString()
            => $"({ParameterExpressions.Select(p => p.ToDisplayString()).StringJoin(", ")}) => {BodyExpression.ToDisplayString()}";
    }


    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LambdaParameterBindingParserNode : BindingParserNode
    {
        public BindingParserNode? Type { get; private set; }
        public BindingParserNode Name { get; private set; }
        public Type? ResolvedType { get; private set; }

        public LambdaParameterBindingParserNode(BindingParserNode? type, BindingParserNode name)
        {
            Type = type;
            Name = name;
            ResolvedType = null;
        }

        public void SetResolvedType(Type argumentType)
            => ResolvedType = argumentType;

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat((Type != null) ? Type.EnumerateNodes().Concat(Name.EnumerateNodes()) : Name.EnumerateNodes());

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => (Type != null) ? new[] { Type, Name } : new[] { Name };

        public override string ToDisplayString()
            => $"{Type?.ToDisplayString()} {Name.ToDisplayString()}";
    }
}
