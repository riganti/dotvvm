using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

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
                if (n is JsSymbolicParameter symExpr && symExpr.Symbol is JsTemporaryVariableParameter parameter)
                {
                    if (allVariables.TryGetValue(parameter, out var currentInterval))
                        allVariables[parameter] = (Math.Min(currentInterval.from, eulerPath.IndexOf((symExpr, true))), Math.Max(currentInterval.to, eulerPath.IndexOf((symExpr, false))));
                    else allVariables.Add(parameter, (parameter.Initializer == null ? eulerPath.IndexOf((symExpr, true)) : 0, eulerPath.IndexOf((symExpr, false))));
                }
                if (n is JsIdentifierExpression identifierExpression)
                {
                    usedNames.Add(identifierExpression.Identifier);
                }
            }

            if (allVariables.Count == 0) return node;

            // TODO a?(b = 5 && b + 2):a
            // a + a + (b)
            //bool intersects(JsTemporaryVariableParameter a, JsTemporaryVariableParameter b) =>
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

            var namedGroups = groups.Zip(GetNames().Where(n => !usedNames.Contains(n)), (g, name) => (vars: g.Value, name: name)).ToArray();
            foreach (var group in namedGroups)
            {
                foreach (var symbolNode in node.DescendantNodesAndSelf().OfType<JsSymbolicParameter>().Where(s => s.Symbol is JsTemporaryVariableParameter p && group.vars.Contains(p)))
                    symbolNode.ReplaceWith(new JsIdentifierExpression(group.name));
            }
            JsNode iife = new JsFunctionExpression(namedGroups.OrderBy(g => !g.vars.Any(v => v.Initializer != null)).Select(g => new JsIdentifier(g.name)),
                node is JsBlockStatement block ? block :
                node is JsStatement statement ? new JsBlockStatement(statement) :
                node is JsExpression expression ? new JsBlockStatement(new JsReturnStatement(expression)) :
                throw new Exception()).Invoke(namedGroups.Select(g => g.vars.SingleOrDefault(v => v.Initializer != null)?.Initializer).Where(v => v != null));
            if (node is JsStatement) iife = new JsExpressionStatement((JsExpression)iife);
            return iife;
        }

        public static IEnumerable<string> GetNames(string baseName = null)
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
        public JsExpression Initializer { get; }
        public string PreferredName { get; }

        public JsTemporaryVariableParameter(JsExpression initializer = null)
            : base("tmp_var[" + initializer?.ToString() + "]")
        {
            this.Initializer = initializer;
        }
    }
}
