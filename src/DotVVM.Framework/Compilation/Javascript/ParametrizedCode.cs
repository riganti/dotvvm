using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using System.Collections.Immutable;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class ParametrizedCode
    {
        private readonly string[] stringParts;
        private readonly CodeParameterInfo[] parameters;
        public readonly OperatorPrecedence OperatorPrecedence;

        public ParametrizedCode(string[] stringParts, CodeParameterInfo[] parameters, OperatorPrecedence operatorPrecence)
        {
            this.stringParts = stringParts;
            this.parameters = parameters;
            this.OperatorPrecedence = operatorPrecence;
        }

        public ParametrizedCode(string code, OperatorPrecedence precedence = new OperatorPrecedence())
            : this(new[] { code }, null, precedence)
        {
        }

        // TODO(exyi): add WriteTo(StringBuilder)
        public string ToString(Func<object, CodeParameterAssignment> parameterAssignment)
        {
            if (stringParts.Length == 1) return stringParts[0];

            var codes = FindStringAssignment(parameterAssignment);

            var sb = new StringBuilder(codes.Sum((p) => p.code.Length) + stringParts.Sum(p => p.Length));
            sb.Append(stringParts[0]);
            for (int i = 0; i < codes.Length;)
            {
                var isGlobalContext = codes[i].parameter.IsGlobalContext && parameters[i].IsSafeMemberAccess;
                var needsParens = codes[i].parameter.Code.OperatorPrecedence.NeedsParens(parameters[i].OperatorPrecedence);

                if (isGlobalContext)
                    sb.Append(stringParts[++i], 1, stringParts[i].Length - 1); // skip `.`
                else
                {
                    if (needsParens) sb.Append("(");
                    sb.Append(codes[i].code);
                    if (needsParens) sb.Append(")");
                    sb.Append(stringParts[++i]);
                }
            }
            return sb.ToString();
        }

        public ParametrizedCode AssignParameters(Func<object, CodeParameterAssignment> parameterAssignement)
        {
            if (stringParts.Length == 1) return this;

            // PERF: reduce allocations here, used at runtime
            var assignment = FindAssignment(parameterAssignement, optional: true);
            var builder = new Builder();

            builder.Add(stringParts[0]);
            for (int i = 0; i < assignment.Length; i++)
            {
                var isGlobalContext = assignment[i].IsGlobalContext && parameters[i].IsSafeMemberAccess;

                if (isGlobalContext)
                    builder.Add(stringParts[1 + i].Substring(1, stringParts[i].Length - 1)); // skip `.`
                else
                {
                    builder.Add(assignment[i].Code, parameters[i].OperatorPrecedence);
                    builder.Add(stringParts[++i]);
                }
            }

            return builder.Build(OperatorPrecedence);
        }

        public void CopyTo(Builder builder)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                builder.Add(stringParts[i]);
                builder.Add(parameters[i]);
            }
            builder.Add(stringParts.Last());
        }

        private (CodeParameterAssignment parameter, string code)[] FindStringAssignment(Func<object, CodeParameterAssignment> parameterAssigner)
        {
            var pp = FindAssignment(parameterAssigner, optional: false);
            var codes = new(CodeParameterAssignment parameter, string code)[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                codes[i] = (pp[i], pp[i].Code.ToString(parameterAssigner));
            }
            return codes;
        }

        private CodeParameterAssignment[] FindAssignment(Func<object, CodeParameterAssignment> parameterAssigner, bool optional)
        {
            var pp = new CodeParameterAssignment[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if ((pp[i] = parameterAssigner(parameters[i].Parameter)).Code == null && !optional)
                    throw new InvalidOperationException($"Assignment of paremeter '{parameters[i].Parameter}' was not found.");
            }
            return pp;
        }

        public class Builder : System.Collections.IEnumerable
        {
            private readonly List<string> stringParts = new List<string>();
            private readonly List<CodeParameterInfo> parameters = new List<CodeParameterInfo>();

            public void Add(string code)
            {
                if (stringParts.Count > parameters.Count)
                    stringParts[stringParts.Count - 1] = stringParts[stringParts.Count - 1] + code;
                else stringParts.Add(code);
            }

            public void Add(CodeParameterInfo parameter)
            {
                if (parameters.Count >= stringParts.Count) stringParts.Add(string.Empty);
                parameters.Add(parameter);
            }

            public void Add(ParametrizedCode code, byte operatorPrecedence = 20)
            {
                var needsParens = code.OperatorPrecedence.NeedsParens(operatorPrecedence);
                if (needsParens) Add("(");
                code.CopyTo(this);
                if (needsParens) Add(")");
            }

            public ParametrizedCode Build(OperatorPrecedence operatorPrecedence) =>
                new ParametrizedCode(stringParts.ToArray(), parameters.ToArray(), operatorPrecedence);

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                for (int i = 0; i < stringParts.Count; i++)
                {
                    yield return stringParts[i];
                    if (parameters.Count < i) yield return parameters[i];
                }
            }
        }
    }

    public struct CodeParameterInfo
    {
        public readonly object Parameter;
        /// <summary>
        /// Operator precedence of the top expression to make sure that the parameter is correctly parenthised.
        /// </summary>
        public readonly byte OperatorPrecedence;
        public readonly bool IsSafeMemberAccess;

        public CodeParameterInfo(object parameter, byte operatorPrecence = 20, bool isMemberAccess = false)
        {
            this.Parameter = parameter;
            this.OperatorPrecedence = operatorPrecence;
            this.IsSafeMemberAccess = isMemberAccess;
        }

        public static CodeParameterInfo FromExpression(JsSymbolicParameter expression)
        {
            return new CodeParameterInfo(expression.Symbol, JsParensFixingVisitor.OperatorLevel(expression.Parent as JsExpression), expression.Parent is JsMemberAccessExpression);
        }
    }

    public struct CodeParameterAssignment
    {
        public readonly ParametrizedCode Code;
        public readonly bool IsGlobalContext;

        public CodeParameterAssignment(string code, OperatorPrecedence operatorPrecedence, bool isGlobalContext = false)
            : this(new ParametrizedCode(new[] { code }, null, operatorPrecedence), isGlobalContext) { }
        public CodeParameterAssignment(ParametrizedCode code, bool isGlobalContext = false)
        {
            this.Code = code;
            this.IsGlobalContext = isGlobalContext;
        }

        public static CodeParameterAssignment FromExpression(JsExpression expression, bool isGlobalContext = false)
        {
            var code = expression.FormatScript();
            var op = JsParensFixingVisitor.GetOperatorPrecedence(expression);
            return new CodeParameterAssignment(code, op, isGlobalContext);
        }
    }
}
