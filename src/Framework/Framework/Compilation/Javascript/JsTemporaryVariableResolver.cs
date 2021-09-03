using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using System.Diagnostics;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JsTemporaryVariableResolver
    {
        public static JsNode ResolveVariables(JsNode node)
        {
            void walkNode(List<(JsNode, bool)> path, JsNode n)
            {
                path.Add((n, true));
                foreach (var c in n.Children)
                    walkNode(path, c);
                path.Add((n, false));
            }
            var eulerPath = new List<(JsNode node, bool isFirst)>();
            walkNode(eulerPath, node);

            var allVariables = new Dictionary<JsTemporaryVariableParameter, (int from, int to)>();
            var usedNames = new HashSet<string>();
            foreach (var n in node.DescendantNodesAndSelf())
            {
                if (n is JsSymbolicParameter symExpr)
                {
                    foreach (var parameter in symExpr.EnumerateAllSymbols().OfType<JsTemporaryVariableParameter>())
                    {
                        if (allVariables.TryGetValue(parameter, out var currentInterval))
                        {
                            allVariables[parameter] = (Math.Min(currentInterval.from, eulerPath.IndexOf((symExpr, true))), Math.Max(currentInterval.to, eulerPath.IndexOf((symExpr, false))));
                        }
                        else
                        {
                            allVariables.Add(parameter, (parameter.Initializer == null ? eulerPath.IndexOf((symExpr, true)) : 0, eulerPath.IndexOf((symExpr, false))));
                        }
                    }
                }
                if (n is JsIdentifierExpression identifierExpression)
                {
                    usedNames.Add(identifierExpression.Identifier);
                }
            }

            // inline variables which occur just once
            foreach (var v in allVariables.Keys.ToArray())
            {
                var count = node.DescendantNodesAndSelf()
                    .OfType<JsSymbolicParameter>()
                    .SelectMany(s => s.EnumerateAllSymbols())
                    .OfType<JsTemporaryVariableParameter>()
                    .Count(p => p == v);
                Debug.Assert(count >= 1);
                if (count == 1 && v.AllowInlining && v.Initializer is object)
                {
                    node = node.AssignParameters(p => p == v ? v.Initializer.Clone() : null);
                    allVariables.Remove(v);
                }
            }

            if (allVariables.Count == 0) return node;

            var groups = new SortedDictionary<int, List<JsTemporaryVariableParameter>>();
            foreach (var k in allVariables.OrderBy(k => k.Value.from))
            {
                if (groups.Count > 0 && groups.First() is var first && first.Key < k.Value.from && k.Key.Initializer == null)
                {
                    groups.Remove(first.Key);
                    first.Value.Add(k.Key);
                    groups.Add(k.Value.to, first.Value);
                }
                else
                {
                    groups.Add(k.Value.to, new List<JsTemporaryVariableParameter> { k.Key });
                }
            }

            var namedGroups = groups.Zip(GetNames().Where(n => !usedNames.Contains(n)), (g, name) => {
                var preferredName = g.Value.Select(v => v.PreferredName).FirstOrDefault(n => n is object);
                if (preferredName is object)
                {
                    name = GetNames(preferredName).First(n => !usedNames.Contains(n));
                }

                usedNames.Add(name);
                return (vars: g.Value, name: name);
            }).ToArray();
            foreach (var group in namedGroups)
            {
                node = node.AssignParameters(p =>
                    p is JsTemporaryVariableParameter v && group.vars.Contains(v)
                        ? new JsIdentifierExpression(group.name)
                        : default
                );
            }
            var wrapperBlock =
                node is JsStatement statement ? statement.AsBlock() :
                node is JsExpression expression ? expression.Return().AsBlock() :
                throw new Exception();


            var firstNode = wrapperBlock.Body.FirstOrNullObject();
            foreach (var g in namedGroups)
            {
                var variableDef = new JsVariableDefStatement(g.name, g.vars.SingleOrDefault(v => v.Initializer != null)?.Initializer);
                wrapperBlock.Body.InsertBefore(firstNode, variableDef);
            }

            if (node is JsStatement)
                return wrapperBlock;
            else
            {
                var isAsync = ContainsAwait(node);
                var iife = JsArrowFunctionExpression.CreateIIFE(wrapperBlock, isAsync: isAsync);
                iife.AddAnnotation(new ResultIsPromiseAnnotation(e => e));
                return iife;
            }
        }

        static bool ContainsAwait(JsNode node) =>
            node.DescendantNodesAndSelf(child => !(child is JsFunctionExpression))
                .Any(child => child is JsUnaryExpression { Operator: UnaryOperatorType.Await });

        public static IEnumerable<string> GetNames(string? baseName = null)
        {
            IEnumerable<char> getChars(bool isFirst)
            {
                if (!isFirst)
                {
                    for (char i = '0'; i <= '9'; i++) yield return i;
                    yield return '$';
                }
                for (char i = 'a'; i <= 'z'; i++) yield return i;
                for (char i = 'A'; i <= 'Z'; i++) yield return i;
                yield return '_';
                // TODO unicode :P
            }
            if (baseName != null) yield return baseName;
            foreach (var c in getChars(baseName == null)) yield return baseName + c.ToString();
            foreach (var name in GetNames(baseName))
            {
                foreach (var c in getChars(false)) yield return name + c;
            }
        }

    }
    public sealed class JsTemporaryVariableParameter: CodeSymbolicParameter
    {
        public JsExpression? Initializer { get; }
        public string? PreferredName { get; }
        public bool AllowInlining { get; }

        public JsTemporaryVariableParameter(JsExpression? initializer = null, string? preferredName = null, bool allowInlining = true)
            : base("tmp_var[" + initializer?.ToString() + "]")
        {
            this.Initializer = initializer;
            this.PreferredName = preferredName;
            this.AllowInlining = allowInlining;
        }
    }
}
