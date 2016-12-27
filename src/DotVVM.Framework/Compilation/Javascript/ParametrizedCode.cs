using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class ParametrizedCode
    {
        private readonly string[] stringParts;
        private readonly CodeParameterInfo[] parameters;

        public ParametrizedCode(string[] stringParts, CodeParameterInfo[] parameters)
        {
            this.stringParts = stringParts;
            this.parameters = parameters;
        }

        public string ToString(Func<object, CodeParameterAssignment> parameterAssignment)
        {
            if (stringParts.Length == 1) return stringParts[0];

            var codes = FindStringAssignment(parameterAssignment);

            var sb = new StringBuilder(codes.Sum((p) => p.code.Length) + stringParts.Sum(p => p.Length));
            sb.Append(stringParts[0]);
            for (int i = 0; i < codes.Length;) {
                var isGlobalContext = codes[i].parameter.IsGlobalContext && parameters[i].IsSafeMemberAccess;
                var needsParens = codes[i].parameter.OperatorPrecedence.NeedsParens(parameters[i].OperatorPrecedence);

                if (isGlobalContext)
                    sb.Append(stringParts[++i], 1, stringParts[i].Length - 1); // skip `.`
                else {
                    if (needsParens) sb.Append("(");
                    sb.Append(codes[i].code);
                    if (needsParens) sb.Append(")");
                    sb.Append(stringParts[++i]);
                }
            }
            return sb.ToString();
        }

        private (CodeParameterAssignment parameter, string code)[] FindStringAssignment(Func<object, CodeParameterAssignment> parameterAssigner)
        {
            var pp = FindAssignment(parameterAssigner);
            var codes = new(CodeParameterAssignment parameter, string code)[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                codes[i] = (pp[i], pp[i].Code.ToString(parameterAssigner));
            }
            return codes;
        }

        private CodeParameterAssignment[] FindAssignment(Func<object, CodeParameterAssignment> parameterAssigner)
        {
            var pp = new CodeParameterAssignment[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                if ((pp[i] = parameterAssigner(parameters[i].Parameter)).Code == null)
                    throw new InvalidOperationException($"Assignment of paremeter '{parameters[i].Parameter}' was not found.");
            }
            return pp;
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
        public readonly OperatorPrecedence OperatorPrecedence;
        public readonly bool IsGlobalContext;

        public CodeParameterAssignment(string code, OperatorPrecedence operatorPrecedence, bool isGlobalContext = false)
            : this(new ParametrizedCode(new[] { code }, null), operatorPrecedence, isGlobalContext) { }
        public CodeParameterAssignment(ParametrizedCode code, OperatorPrecedence operatorPrecedence, bool isGlobalContext = false)
        {
            this.Code = code;
            this.OperatorPrecedence = operatorPrecedence;
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
