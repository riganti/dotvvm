using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;
using DotVVM.Framework.Tests.Binding;
using DotVVM.Framework.Tests.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation.Binding;
using System.Diagnostics;
using Newtonsoft.Json;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Testing;
using FastExpressionCompiler;

namespace DotVVM.Framework.Tests.Binding
{
    [TestClass]
    public class NullPropagationTests
    {

        private LambdaExpression[] ExpressionFragments = new LambdaExpression[] {
            Create((TestViewModel t) => t.EnumProperty - 1),
            Create((TestViewModel t) => t.StringProp),
            Create((TestViewModel t) => t.LongArray),
            Create((TestViewModel t) => t.LongProperty),
            Create((TestViewModel t) => t.VmArray),
            Create((TestViewModel t) => t.VmArray[0]),
            Create((TestViewModel2[] t, int b) => t[b & 0]),
            Create((TestViewModel2 t) => t.Struct),
            Create((TestViewModel2 t) => t.ToString()),
            Create((TestViewModel2 t) => t.MyProperty),
            Create((TestStruct t) => t.Int),
            Create((long[] t) => t.Length),
            Create((long[] t) => t[0]),
            Create((TestViewModel2 t) => t.Enum),
            Create((TestEnum t) => (int)t),
            Create((TestEnum t) => (TestEnum?)t),
            Create((TestViewModel a, TestViewModel b) => a.BoolMethodExecuted ? a : b),
            Create((int a, TestViewModel b, TestViewModel c) => a == 0 ? b : c),
            Create((string b) => b ?? "NULL STRING"),
            Create((TestViewModel a, int b) => a.Identity(b)),
            Create((TestViewModel a, DateTime b) => a.Identity<DateTime?>(b)),
            Create((TestViewModel a, string b) => a.Identity(b)),
            Create((TestViewModel a, TestViewModel b) => a.Identity(b)),
            Create((TestViewModel a, char b) => a.GetCharCode(b)),
            Create((string a) => a.Length > 0 ? a[0] : 'f'),
            Create((string a, int b) => a.Length > 0 ? a[b % 1] : 'l'),
            // Create((string a, int b) => a[b]),
            Create((int a, int b) => a+b),
            Create((int a) => a + 1.0),
            Create((long a) => (int)a),
            Create((int a, double b) => a * b),
            Create((TimeSpan span) => span.TotalMilliseconds),
            Create((TestViewModel vm) => vm.DateFrom), // DateTime?
            Create((DateTime? vm) => vm.HasValue),
            Create((DateTime? vm) => vm.Value),
            Create((DateTime? vm) => vm.GetValueOrDefault()),
            Create((DateTime d) => d.Day),
            Create((DateTime d) => d.ToString()),
            Create((double a) => TimeSpan.FromSeconds(a)),
            Create((TestViewModel vm) => (TimeSpan)vm.Time),
            Create((TestViewModel vm, TimeSpan time, int integer, double number, DateTime d) => new TestViewModel {
                EnumProperty = TestEnum.B,
                BoolMethodExecuted = (bool?)vm.BoolMethodExecuted ?? false,
                StringProp = time + ": " + vm.StringProp,
                StringProp2 = integer + vm.StringProp2 + number,
                TestViewModel2 = vm.TestViewModel2,
                DateFrom = d
            }),
        };

        private static LambdaExpression Create<T1, T2>(Expression<Func<T1, T2>> e) => e;
        private static LambdaExpression Create<T1, T2, T3>(Expression<Func<T1, T2, T3>> e) => e;
        private static LambdaExpression Create<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4>> e) => e;
        private static LambdaExpression Create<T1, T2, T3, T4, T5>(Expression<Func<T1, T2, T3, T4, T5>> e) => e;
        private static LambdaExpression Create<T1, T2, T3, T4, T5, T6>(Expression<Func<T1, T2, T3, T4, T5, T6>> e) => e;

        IEnumerable<Expression> FuzzExpressions(Random random, params ParameterExpression[] sources)
        {
            var unusedFragments = new HashSet<LambdaExpression>(ExpressionFragments);
            var typeSources = sources.ToDictionary(s => s.Type, s => new List<Expression> { s });

            IEnumerable<Expression[]> ListParameters(Type[] types)
            {
                var counts = types.Select(t => typeSources[t].Count).ToArray();
                var sequences = types.Select(t => typeSources[t].Shuffle(random, infinite: true).GetEnumerator()).ToArray();
                foreach (var s in sequences) s.MoveNext();
                var index = new int[types.Length];
                while (true)
                {
                    var current = new Expression[types.Length];
                    for (int i = 0; i < index.Length; i++)
                    {
                        current[i] = sequences[i].Current;
                    }
                    yield return current;

                    index[0]++;
                    sequences[0].MoveNext();
                    for (var i = 0; i < index.Length; i++)
                        if (index[i] == counts[i])
                        {
                            if (i + 1 == index.Length) yield break;
                            index[i] = 0;
                            index[i + 1]++;
                            sequences[i + 1].MoveNext();
                        }
                        else break;
                    if (index[index.Length - 1] > 0) break;
                }
            }

            void AddFragment(LambdaExpression fragment, int maxCount)
            {
                var newOnes = ListParameters(fragment.Parameters.Select(p => p.Type).ToArray()).Take(maxCount)
                    .Select(p => ExpressionUtils.Replace(fragment, p));
                foreach (var item in newOnes)
                {
                    if (!typeSources.TryGetValue(item.Type, out var list)) typeSources.Add(item.Type, list = new List<Expression>());
                    list.Add(item);
                }
            }

            // spawn all possible types
            while (unusedFragments.Any())
            {
                var possibleOnes = unusedFragments.Where(f => f.Parameters.All(p => typeSources.ContainsKey(p.Type))).ToArray();
                Assert.IsFalse(possibleOnes.Length == 0, $"Cannot continue from {string.Join(", ", typeSources.Select(t => t.Key.Name))} to {string.Join(", ", unusedFragments.AsEnumerable())}");
                foreach (var fragment in possibleOnes)
                {
                    AddFragment(fragment, 4);
                    unusedFragments.Remove(fragment);
                }
            }

            // spawn few more

            foreach (var ff in ExpressionFragments.Shuffle(random, infinite: true).Take(5))
            {
                AddFragment(ff, 8);
            }
            return typeSources.Values.SelectMany(_ => _);
        }

        private void TestExpression(Random rnd, Expression expression, ParameterExpression originalParameter)
        {
            var parameter = Expression.Parameter(typeof(TestViewModel[]), "par");
            var count = 0;
            var expr = new ReplacerVisitor(originalParameter, () => Expression.ArrayIndex(parameter, Expression.Constant(count++))).Visit(expression);
            var exprWithChecks = ExpressionNullPropagationVisitor.PropagateNulls(expr, _ => true);

            Func<TestViewModel[], object> compile(Expression e) =>
                Expression.Lambda<Func<TestViewModel[], object>>(Expression.Convert(e, typeof(object)), parameter)
                    // .CompileFast();
                    .Compile(preferInterpretation: true);

            var withNullChecks = compile(exprWithChecks);
            var withoutNullChecks = compile(expr);

            var args = Enumerable.Repeat(new TestViewModel { StringProp = "ll", StringProp2 = "pp", TestViewModel2 = new TestViewModel2(), DateFrom = DateTime.Parse("2020-03-29") }, count).ToArray();
            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
            object resultWithoutChecks;
            object resultWithChecks;
            try
            {
                resultWithoutChecks = withoutNullChecks(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Original expression: {expr.ToCSharpString()}");
                Console.WriteLine();
                Console.WriteLine(expr.ToExpressionString());
                Console.WriteLine(e.ToString());
                Assert.Fail($"Exception {e.Message} while executing (without null-checks) {expr.ToCSharpString()}");
                return;
            }
            try
            {
                resultWithChecks = withNullChecks(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Original expression: {expr.ToCSharpString()}");
                Console.WriteLine($"Null-checked expression {expr.ToCSharpString()}");
                Console.WriteLine();
                Console.WriteLine(expr.ToExpressionString());
                Console.WriteLine(e.ToString());
                Assert.Fail($"Exception {e.Message} while executing null-checked {expr.ToCSharpString()}");
                return;
            }
            Assert.AreEqual(JsonConvert.SerializeObject(withNullChecks(args), settings), JsonConvert.SerializeObject(withoutNullChecks(args), settings));

            foreach (var i in Enumerable.Range(0, args.Length).Shuffle(rnd))
            {
                args[i] = null;
                try
                {
                    withNullChecks(args);
                }
                catch (NullReferenceException ex)
                {
                    // we might get exceptions from B(null), ...
                    // none of the methods we call actually throws, so it always has to be ours friendly NRE
                    Assert.IsTrue(ex.Message.StartsWith("Binding expression"));
                    // and it must only occur for value types
                    Assert.IsTrue(new [] { "System.Int", "System.TimeSpan", "System.Char", "System.Double" }.Any(ex.Message.Contains));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Original expression: {expr.ToCSharpString()}");
                    Console.WriteLine($"Null-checked expression {expr.ToCSharpString()}");
                    Console.WriteLine();
                    Console.WriteLine(expr.ToExpressionString());
                    Console.WriteLine(e.ToString());
                    Assert.Fail($"Exception {e.Message} while executing null-checked {expr.ToCSharpString()}");
                    return;
                }
            }
        }

        private object EvalExpression<T>(Expression<Func<T, object>> a, T val)
        {
            var nullChecked = ExpressionNullPropagationVisitor.PropagateNulls(a.Body, _ => true);
            var d = a.Update(body: nullChecked, a.Parameters).Compile(preferInterpretation: true);
            return d(val);
        }

        class ReplacerVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression originalParameter;
            private readonly Func<Expression> replacement;

            private readonly HashSet<ParameterExpression> undeclaredVariables = new HashSet<ParameterExpression>();
            private readonly HashSet<ParameterExpression> declaredVariables = new HashSet<ParameterExpression>();

            public ReplacerVisitor(ParameterExpression originalParameter, Func<Expression> replacement)
            {
                this.originalParameter = originalParameter;
                this.replacement = replacement;
            }

            public new Expression Visit(Expression expr)
            {
                var r = base.Visit(expr);
                if (undeclaredVariables.Any())
                {
                    return Expression.Block(undeclaredVariables, r);
                }
                else return r;
            }

            protected override Expression VisitBlock(BlockExpression node)
            {
                foreach (var v in node.Variables)
                {
                    declaredVariables.Add(v);
                }
                return base.VisitBlock(node);
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (originalParameter == node) return replacement();
                else
                {
                    if (!declaredVariables.Contains(node))
                        undeclaredVariables.Add(node);
                    return base.VisitParameter(node);
                }
            }
        }

        [TestMethod]
        public void BindingNullPropagation_1()
        {
            var viewModel = Expression.Parameter(typeof(TestViewModel), "viewModel");
            var seed = (int)DateTime.Now.Ticks;
            var random = new Random(seed);
            Debug.WriteLine("FuzzExpressions seed = " + seed);

            var expressions = FuzzExpressions(random, viewModel)
                .OrderBy(e => e.ToCSharpString().Length) // shorter first - we are generating expressions by combining them, so we will fail at the first combination of some parts that fails
                .ToArray();

            foreach (var expr in expressions)
            {
                TestExpression(random, expr, viewModel);
            }
        }

        private static T Identity<T>(T x) => x;

        [TestMethod]
        public void MethodArgument_Null_ValueType()
        {
            var e = Assert.ThrowsException<NullReferenceException>(() =>
                EvalExpression<TestViewModel>(v => Identity(v.LongProperty), null));
            Assert.AreEqual("Binding expression 'v.LongProperty' of type 'System.Int64' has evaluated to null.", e.Message);
        }

        [TestMethod]
        public void MethodArgument_Null_NullableType()
        {
            Assert.IsNull(EvalExpression<TestViewModel>(v => Identity(v.DateFrom), null));
            Assert.IsNull(EvalExpression<TestViewModel>(v => Identity<long?>(v.LongProperty), null));
            Assert.IsNull(EvalExpression<TestViewModel>(v => Identity<DateTime?>(v.DateFrom.Value), null));
        }

        [TestMethod]
        public void MethodArgument_Null_RefType()
        {
            Assert.IsNull(EvalExpression<TestViewModel>(v => Identity(v), null));
            Assert.IsNull(EvalExpression<TestViewModel>(v => Identity(v.LongArray), null));
        }

        [TestMethod]
        public void Operator()
        {
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.LongArray[0] + 1, null));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.LongArray[0] + v.TestViewModel2.MyProperty, new TestViewModel()));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.TestViewModel2B.ChildObject.SomeString.Length + v.TestViewModel2.MyProperty, new TestViewModel()));
            Assert.AreEqual(2L, EvalExpression<TestViewModel>(v => v.LongArray[0] + 1, new TestViewModel()));
        }

        [TestMethod]
        public void StringConcat()
        {
            Assert.AreEqual("abc", EvalExpression<TestViewModel>(v => v.StringProp + "abc", null));
        }

        [TestMethod]
        public void Indexer()
        {
            Assert.IsNull(EvalExpression<int[]>(v => v[0] + 1, null));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.IntArray[0] + 1, null));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.IntArray[0] + 1, new TestViewModel { IntArray = null }));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.TestViewModel2.Collection[0].StringValue.Length + 5, new TestViewModel { IntArray = null }));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.TestViewModel2.Collection[0].StringValue.Length + 5, new TestViewModel { IntArray = null }));
        }

        [TestMethod]
        public void ExtensionMethod()
        {
            Assert.IsNull(EvalExpression<int[]>(v => v.Where(v => v % 2 == 0).ToArray(), null));
            Assert.IsNull(EvalExpression<int[]>(v => v.Where(v => v % 2 == 0), null));
            Assert.IsNull(EvalExpression<int[]>(v => v.Where(v => v % 2 == 0).Count(), null));
            Assert.AreEqual(1, EvalExpression<int[]>(v => v.Where(v => v % 2 == 0).Count(), new [] { 1, 2 }));
            Assert.IsNull(EvalExpression<Tuple<DateTime>>(v => v.Item1.ToBrowserLocalTime(), null));
            Assert.IsNull(EvalExpression<Tuple<DateTime?>>(v => v.Item1.ToBrowserLocalTime(), null));
            Assert.IsNull(EvalExpression<DateTime?>(v => v.ToBrowserLocalTime(), null));
        }

        [TestMethod]
        public void IndexerArgument()
        {
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.IntArray[v.NullableIntProp.Value], new TestViewModel()));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.TestViewModel2.Collection[v.NullableIntProp.Value], new TestViewModel()));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.StringVmDictionary[v.StringProp], new TestViewModel()));
            Assert.IsNull(EvalExpression<TestViewModel>(v => v.NullableIntVmDictionary[v.NullableIntProp], new TestViewModel()));
        }

        [TestMethod]
        public void Coalesce()
        {
            Assert.AreEqual(1, EvalExpression<object>(v => v ?? 1, null));
            Assert.AreEqual(1, EvalExpression<object>(v => (v ?? 1) ?? 2, null));
            Assert.AreEqual(1, EvalExpression<int?>(v => v ?? null, 1));
        }

        [TestMethod]
        public void ValueTypePropertyAccess()
        {
            var ex = Assert.ThrowsException<NullReferenceException>(() =>
                EvalExpression<TestViewModel>(v => TimeSpan.FromSeconds(v.IntProp).TotalMilliseconds, null)
            );
            var convertExpression = (TestEnvironmentHelper.GetFrameworkType() == TestEnvironmentHelper.FrameworkType.Net)
                ? "Convert(v.IntProp, Double)" : "Convert(v.IntProp)";
            Assert.AreEqual($"Binding expression '{convertExpression}' of type 'System.Double' has evaluated to null.", ex.Message);

            Assert.AreEqual(1000d, EvalExpression<TestViewModel>(v => TimeSpan.FromSeconds(v.IntProp).TotalMilliseconds, new TestViewModel { IntProp = 1 }));
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, bool infinite = false)
        {
            return source.Shuffle(new Random(), infinite);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng, bool infinite = false)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng, infinite);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng, bool infinite = false)
        {
            var buffer = source.ToList();
            do
            {
                for (int i = 0; i < buffer.Count; i++)
                {
                    int j = rng.Next(i, buffer.Count);
                    yield return buffer[j];

                    buffer[j] = buffer[i];
                }
            }
            while (infinite);
        }
    }
}
