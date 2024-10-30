using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using System.Collections.Immutable;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Binding.Expressions;
using Newtonsoft.Json;
using System.Diagnostics;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript
{
    /// <summary>
    /// Represents a piece of Javascript code that may contain unresolved symbolic parameters.
    /// </summary>
    public sealed class ParametrizedCode
    {
        private readonly string[]? stringParts;
        private readonly CodeParameterInfo[]? parameters;
        private string? evaluatedDefault;
        public readonly OperatorPrecedence OperatorPrecedence;

        public bool HasParameters => parameters != null && parameters.Length > 0;
        public IEnumerable<CodeParameterInfo> Parameters =>
            // make sure that it's really immutable
            parameters?.Select(p => p) ?? Enumerable.Empty<CodeParameterInfo>();

        public ParametrizedCode(string[]? stringParts, CodeParameterInfo[]? parameters, OperatorPrecedence operatorPrecedence, string? evaluatedDefault = null)
        {
            if (stringParts == null)
                this.evaluatedDefault = evaluatedDefault ?? throw new ArgumentNullException(nameof(stringParts), "Can't be null, unless evaluatedDefauls is set.");
            else if (stringParts.Length == 1)
                this.evaluatedDefault = stringParts[0] ?? throw new ArgumentNullException(nameof(stringParts), "Can't be null, unless evaluatedDefauls is set.");
            else
            {
                this.stringParts = stringParts;
                this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters), "Can't be null, unless stringParts.Length == 1");
                this.evaluatedDefault = evaluatedDefault;
            }
            this.OperatorPrecedence = operatorPrecedence;
        }

        public ParametrizedCode(string code, OperatorPrecedence precedence = default)
        {
            this.evaluatedDefault = code ?? throw new ArgumentNullException(nameof(code));
            this.OperatorPrecedence = precedence;
        }

        // TODO(exyi): add WriteTo(StringBuilder)
        /// <summary>
        /// Converts this to string and assigns all parameters using `parameterAssignment`. If there is any missing, exception is thrown.
        /// </summary>
        public string ToString(Func<CodeSymbolicParameter, CodeParameterAssignment> parameterAssignment) => ToString(parameterAssignment, out var _);
        public string ToString(Func<CodeSymbolicParameter, CodeParameterAssignment> parameterAssignment, out bool allIsDefault)
        {
            allIsDefault = true;
            if (stringParts == null) return evaluatedDefault!;
            Debug.Assert(parameters is object);

            var codes = FindStringAssignment(parameterAssignment, out allIsDefault);

            if (allIsDefault && this.evaluatedDefault != null)
                return evaluatedDefault;

            var sb = new StringBuilder(codes.Sum((p) => p.code.Length) + stringParts.Sum(p => p.Length));
            sb.Append(stringParts[0]);
            for (int i = 0; i < codes.Length;)
            {
                var isGlobalContext = codes[i].parameter.IsGlobalContext && parameters![i].IsSafeMemberAccess;
                var needsParens = codes[i].parameter.Code!.OperatorPrecedence.NeedsParens(parameters![i].OperatorPrecedence);

                if (isGlobalContext)
                    sb.Append(stringParts[++i], 1, stringParts[i].Length - 1); // skip `.`
                else
                {
                    if (needsParens)
                        sb.Append("(");
                    else if (JsFormattingVisitor.NeedSpaceBetween(sb, codes[i].code))
                        sb.Append(" ");
                    sb.Append(codes[i].code);
                    i++;
                    if (needsParens) sb.Append(")");
                    else if (JsFormattingVisitor.NeedSpaceBetween(sb, stringParts[i]))
                        sb.Append(" ");
                    sb.Append(stringParts[i]);
                }
            }
            var result = sb.ToString();
            if (allIsDefault)
                this.evaluatedDefault = result.DotvvmInternString();
            return result;
        }

        [Obsolete("ParametrizedCode.ToString use is discouraged, this overload does not return the code, please use the ToString(Func<...> parameterAssigner) overload or ToDefaultString method. Note that these methods may throw an exception.", true)]
        public new string ToString()
        {
            // leave for debug purposes.
            return ToDebugString();
        }

        public string ToDefaultString()
        {
            if (this.evaluatedDefault != null)
                return this.evaluatedDefault;
            return this.evaluatedDefault = ToString(_ => default);
        }

        public string ToDebugString()
        {
            if (stringParts is null)
                return evaluatedDefault!;
            Debug.Assert(parameters is object);
            var sb = new StringBuilder();
            for (int i = 0; i < parameters!.Length; i++)
            {
                sb.Append(stringParts[i]);
                sb.Append(parameters[i].ToString());
            }
            sb.Append(stringParts.Last());
            return sb.ToString();
        }

        /// <summary>
        /// Assigns parameters and return new ParametrizedCode. If parameter is not assigned, it is copied to the resulting parameter. Assigner can also replace parameter by script that contains another parameters.
        /// </summary>
        public ParametrizedCode AssignParameters(Func<CodeSymbolicParameter, CodeParameterAssignment> parameterAssignment)
        {
            if (stringParts == null) return this;
            Debug.Assert(parameters is object);

            var (assignment, newDefaults) = FindAssignment(parameterAssignment);

            if (assignment is null)
            {
                if (newDefaults is null)
                    return this;
                else
                {
                    // just change the content of parameters, no need to rebuild strings
                    var newParams = parameters.AsSpan().ToArray();
                    for (int i = 0; i < newParams.Length; i++)
                    {
                        ref var p = ref newParams[i];
                        if (newDefaults[i] is { } newDefault)
                            p = new CodeParameterInfo(p.Parameter, p.OperatorPrecedence, p.IsSafeMemberAccess, newDefault);
                    }
                    return new ParametrizedCode(stringParts, newParams, OperatorPrecedence);
                }
            }

            // PERF: reduce allocations here, used at runtime
            var builder = new Builder();

            builder.Add(stringParts[0]);
            for (int i = 0; i < assignment.Length; i++)
            {
                var a = assignment[i];
                var param = parameters![i];
                if (a.Code == null) // not assigned by `parameterAssignment`
                {
                    // assign recursively in the default assignment
                    if (newDefaults is {} && newDefaults[i] is { } newDefault)
                        builder.Add(new CodeParameterInfo(param.Parameter, param.OperatorPrecedence, param.IsSafeMemberAccess, newDefault));
                    else
                        builder.Add(param);

                    builder.Add(stringParts[1 + i]);
                }
                else
                {
                    var isGlobalContext = a.IsGlobalContext && param.IsSafeMemberAccess;

                    if (isGlobalContext)
                        builder.Add(stringParts[1 + i].AsSpan(1, stringParts[1 + i].Length - 1).DotvvmInternString()); // skip `.`
                    else
                    {
                        builder.Add(a.Code, param.OperatorPrecedence);
                        builder.Add(stringParts[1 + i]);
                    }
                }
            }

            return builder.Build(OperatorPrecedence);
        }

        /// <summary>
        /// Writes this code including parameters to the ParametrizedCode.Builder
        /// </summary>
        public void CopyTo(Builder builder)
        {
            if (stringParts == null)
                builder.Add(evaluatedDefault!);
            else
            {
                for (int i = 0; i < parameters!.Length; i++)
                {
                    builder.Add(stringParts[i]);
                    builder.Add(parameters[i]);
                }
                builder.Add(stringParts.Last());
            }
        }

        private (CodeParameterAssignment parameter, string code)[] FindStringAssignment(Func<CodeSymbolicParameter, CodeParameterAssignment> parameterAssigner, out bool allIsDefault)
        {
            allIsDefault = true;
            var codes = new(CodeParameterAssignment parameter, string code)[parameters!.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var assignment = parameterAssigner(parameters[i].Parameter);
                if (assignment.Code == null)
                {
                    assignment = parameters[i].DefaultAssignment;
                    if (assignment.Code == null)
                        throw new InvalidOperationException($"Assignment of parameter '{parameters[i].Parameter}' was not found.");
                }
                else
                    allIsDefault = false;

                codes[i] = (assignment, assignment.Code!.ToString(parameterAssigner, out bool allIsDefault_local));
                allIsDefault &= allIsDefault_local;
            }
            return codes;
        }

        private (CodeParameterAssignment[]? assigned, ParametrizedCode?[]? newDefaults) FindAssignment(Func<CodeSymbolicParameter, CodeParameterAssignment> parameterAssigner)
        {
            if (parameters is null)
                return (null, null);

            // these are different variables, as we have to preserve the tree-like structure of the ParametrizedCodes,
            // when we assign parameters in the default values.
            // newDefaults -> we will change the code in the parameters[i].DefaultAssignment
            // assigned -> this parameter will be removed and its assignment inlined into stringParts[i] and parameters[i]
            CodeParameterAssignment[]? assigned = null; // when null, all are default
            ParametrizedCode[]? newDefaults = null; // when null, no defaults were changed
            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameterAssigner(parameters[i].Parameter);
                if (p.Code is not null)
                {
                    assigned ??= new CodeParameterAssignment[parameters.Length];
                    assigned[i] = p;
                }
                else if (parameters[i].DefaultAssignment is { Code: { HasParameters: true } } defaultAssignment)
                {
                    // check if the default assignment contains any of the assigned parameters, and adjust the default if necessary
                    var newDefault = defaultAssignment.Code.AssignParameters(parameterAssigner);
                    if (newDefault != defaultAssignment.Code)
                    {
                        newDefaults ??= new ParametrizedCode[parameters.Length];
                        newDefaults[i] = newDefault;
                    }
                }
            }
            return (assigned, newDefaults);
        }

        public IEnumerable<CodeSymbolicParameter> EnumerateAllParameters()
        {
            if (this.parameters == null)
                yield break;
            foreach (var p in this.parameters)
            {
                yield return p.Parameter;
                if (p.DefaultAssignment.Code != null)
                {
                    foreach (var inner in p.DefaultAssignment.Code.EnumerateAllParameters())
                        yield return inner;
                }
            }
        }

        /// <summary>
        /// Builder class with reasonably fast Add operation. Use Build method to convert it to immutable ParametrizedCode
        /// </summary>
        public class Builder : System.Collections.IEnumerable
        {
            private readonly List<string> stringParts = new List<string>();
            private readonly StringBuilder lastPart = new StringBuilder();
            private readonly List<CodeParameterInfo> parameters = new List<CodeParameterInfo>();

            public void Add(string code)
            {
                if (JsFormattingVisitor.NeedSpaceBetween(lastPart, code))
                    lastPart.Append(" ");
                lastPart.Append(code);
            }

            public void Add(CodeParameterInfo parameter)
            {
                stringParts.Add(lastPart.ToString().DotvvmInternString());
                lastPart.Clear();
                parameters.Add(parameter);
            }

            public void Add(ParametrizedCode code, byte operatorPrecedence = 20)
            {
                var needsParens = code.OperatorPrecedence.NeedsParens(operatorPrecedence);
                if (needsParens) Add("(");
                code.CopyTo(this);
                if (needsParens) Add(")");
            }

            public ParametrizedCode Build(OperatorPrecedence operatorPrecedence)
            {
                stringParts.Add(lastPart.ToString().DotvvmInternString());
                return new ParametrizedCode(stringParts.ToArray(), parameters.ToArray(), operatorPrecedence);
            }

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

    /// <summary>
    /// Represents an symbolic parameter in the ParametrizedCode.
    /// </summary>
    public readonly struct CodeParameterInfo
    {
        public readonly CodeSymbolicParameter Parameter;
        /// Optional default value
        public readonly CodeParameterAssignment DefaultAssignment;
        /// <summary>
        /// Operator precedence of the top expression to make sure that the parameter is correctly parenthesized.
        /// </summary>
        public readonly byte OperatorPrecedence;
        /// <summary>
        /// If the parameter would be available as global, can it be omitted?
        /// </summary>
        public readonly bool IsSafeMemberAccess;

        public CodeParameterInfo(CodeSymbolicParameter parameter, byte operatorPrecedence = 20, bool isSafeMemberAccess = false, CodeParameterAssignment? assignment = null)
        {
            this.Parameter = parameter;
            this.OperatorPrecedence = operatorPrecedence;
            this.IsSafeMemberAccess = isSafeMemberAccess;
            this.DefaultAssignment = assignment ?? parameter.DefaultAssignment;
        }

        public override string ToString()
        {
            var assignment = DefaultAssignment.Code is object ?
                             " = " + DefaultAssignment.ToString() :
                             "";
            return $"{{{this.Parameter.Description}|{(uint)this.Parameter.GetHashCode()}{assignment}}}";
        }

        public static CodeParameterInfo FromExpression(JsSymbolicParameter expression)
        {
            return new CodeParameterInfo(expression.Symbol, JsParensFixingVisitor.GetParentLevel(expression), expression.Parent is JsMemberAccessExpression, expression.DefaultAssignment);
        }
    }

    public readonly struct CodeParameterAssignment
    {
        public readonly ParametrizedCode? Code;
        public readonly bool IsGlobalContext;

        public CodeParameterAssignment(string code, OperatorPrecedence operatorPrecedence, bool isGlobalContext = false)
            : this(new ParametrizedCode(code, operatorPrecedence), isGlobalContext) { }
        public CodeParameterAssignment(ParametrizedCode? code, bool isGlobalContext = false)
        {
            this.Code = code;
            this.IsGlobalContext = isGlobalContext;
        }


        public override string ToString()
        {
            if (Code is null) return "<empty>";
            return Code.ToDebugString();
        }

        public static CodeParameterAssignment FromExpression(JsExpression expression, bool isGlobalContext = false, bool niceMode = false)
        {
            var code = expression.FormatParametrizedScript(niceMode: niceMode);
            return new CodeParameterAssignment(code, isGlobalContext);
        }

        public static CodeParameterAssignment FromIdentifier(string identifier, bool isGlobalContext = false) =>
            new CodeParameterAssignment(identifier, OperatorPrecedence.Max, isGlobalContext);
        public static CodeParameterAssignment FromLiteral(string value, bool isGlobalContext = false) =>
            new CodeParameterAssignment(JsonConvert.ToString(value), OperatorPrecedence.Max, isGlobalContext);

        public static implicit operator CodeParameterAssignment(ParametrizedCode? val) => new CodeParameterAssignment(val);
    }

    /// (Base) class for symbolic parameter descriptors.
    /// This is mainly a marker class, the parameters are compared by reference equality, but this contains some optional features (default and description).
    public class CodeSymbolicParameter
    {
        public readonly string Description;
        public readonly CodeParameterAssignment DefaultAssignment;
        public bool HasDefault => DefaultAssignment.Code != null;

        public CodeSymbolicParameter(string description = "", CodeParameterAssignment defaultAssignment = default)
        {
            this.Description = description ?? throw new ArgumentNullException(nameof(description));
            this.DefaultAssignment = defaultAssignment;
        }

        public CodeParameterInfo ToInfo(OperatorPrecedence operatorPrecedence) => ToInfo(operatorPrecedence.Precedence);
        public CodeParameterInfo ToInfo(byte operatorPrecedence) => new CodeParameterInfo(this, operatorPrecedence);
        public CodeParameterInfo ToInfo() => ToInfo(this.DefaultAssignment.Code?.OperatorPrecedence ?? OperatorPrecedence.Max);

        public ParametrizedCode ToParametrizedCode(OperatorPrecedence operatorPrecedence) =>
            new ParametrizedCode(new [] { "", "" }, new [] { ToInfo(operatorPrecedence) }, operatorPrecedence);
        public ParametrizedCode ToParametrizedCode() =>
            ToParametrizedCode(this.DefaultAssignment.Code?.OperatorPrecedence ?? OperatorPrecedence.Max);

        public JsSymbolicParameter ToExpression(CodeParameterAssignment? defaultAssignment = null) =>
            new JsSymbolicParameter(this, defaultAssignment);

        public override string ToString()
        {
            var assignment = DefaultAssignment.Code is object ?
                             " = " + DefaultAssignment.ToString() :
                             "";
            return $"{{{this.Description}|{(uint)this.GetHashCode()}{assignment}}}";
        }
    }
}
