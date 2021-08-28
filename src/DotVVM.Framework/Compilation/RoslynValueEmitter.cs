using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation
{
    public class RoslynValueEmitter
    {
        public RoslynValueEmitter(Func<Type, TypeSyntax> useType)
        {
            UseType = useType;
        }

        protected Func<Type, TypeSyntax> UseType { get; }

        private ConditionalWeakTable<ExpressionSyntax, object?> inverseEmitValue = new();

        /// <summary>
        /// Emits the value.
        /// </summary>
        public ExpressionSyntax EmitValue(object? value)
        {
            var result = EmitValueCore(value);
            inverseEmitValue.GetValue(result, _ => value);
            return result;
        }

        public bool TryInvertExpression(ExpressionSyntax expression, out object? value) =>
            inverseEmitValue.TryGetValue(expression, out value);

        private ExpressionSyntax EmitValueCore(object? value)
        {
            if (value == null)
            {
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }
            if (value is string)
            {
                return EmitStringLiteral((string)value);
            }
            if (value is bool)
            {
                return EmitBooleanLiteral((bool)value);
            }
            if (value is int)
            {
                return EmitStandardNumericLiteral((int)value);
            }
            if (value is long)
            {
                return EmitStandardNumericLiteral((long)value);
            }
            if (value is ulong)
            {
                return EmitStandardNumericLiteral((ulong)value);
            }
            if (value is uint)
            {
                return EmitStandardNumericLiteral((uint)value);
            }
            if (value is decimal)
            {
                return EmitStandardNumericLiteral((decimal)value);
            }
            if (value is float)
            {
                return EmitStandardNumericLiteral((float)value);
            }
            if (value is double)
            {
                return EmitStandardNumericLiteral((double)value);
            }
            if (value is Type valueAsType)
            {
                return SyntaxFactory.TypeOfExpression(UseType(valueAsType));
            }

            var type = value.GetType();


            if (ReflectionUtils.IsNumericType(type))
            {
                return EmitStrangeIntegerValue(Convert.ToInt64(value), type);
            }

            if (type.IsEnum)
            {
                UseType(type);
                var flags =
                    value.ToString().Split(',').Select(v =>
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            UseType(type),
                            SyntaxFactory.IdentifierName(v.Trim())
                        )
                   ).ToArray();
                ExpressionSyntax expr = flags[0];
                foreach (var i in flags.Skip(1))
                {
                    expr = SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, expr, i);
                }
                return expr;
            }
            if (IsImmutableObject(type))
                return EmitValueReference(value);
            if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                return EmitCreateArray(ReflectionUtils.GetEnumerableType(type)!, (System.Collections.IEnumerable)value);
            }
            throw new NotSupportedException($"Emitting value of type '{value.GetType().FullName}' is not supported.");
        }

        protected ExpressionSyntax EmitCreateArray(Type elementType, System.Collections.IEnumerable values)
        {
            return DefaultViewCompilerCodeEmitter.EmitCreateArray(UseType(elementType), values.Cast<object>().Select(EmitValue));
        }

        /// <summary>
        /// Emits the boolean literal.
        /// </summary>
        private ExpressionSyntax EmitBooleanLiteral(bool value)
        {
            return SyntaxFactory.LiteralExpression(value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }

        static ConcurrentDictionary<object, (int, string)> bucketIndexCache = new ConcurrentDictionary<object, (int, string)>(new EqCmp());
        class EqCmp : IEqualityComparer<object>, IEqualityComparer
        {
            public new bool Equals(object x, object y)
            {
                if (x is IStructuralEquatable se)
                    return se.Equals(y, this);
                return Object.Equals(x, y);
            }

            public int GetHashCode(object x)
            {
                if (x is IStructuralEquatable se)
                    return se.GetHashCode(this);
                if (x == null) return 5642635;
                return x.GetHashCode();
            }
        }

        public virtual ExpressionSyntax EmitValueReference(object value)
        {
            // var (id, bucket) = AddObjectToStaticField(value);
            var (id, bucket) = bucketIndexCache.GetOrAdd(value, AddObjectToStaticField);

            var result = EmitValueReferenceById(id, bucket, value.GetType());
            inverseEmitValue.GetValue(result, _ => value);
            return result;
        }

        private static (int, string) AddObjectToStaticField(object value)
        {
            return
                value is DotvvmProperty[] propArray ?
                    (AddObject(propArray, ref _ViewImmutableObjects_PropArray, ref _viewObjectsCount_PropArray), nameof(_ViewImmutableObjects_PropArray)) :
                value is object[] objArray ?
                    (AddObject(objArray, ref _ViewImmutableObjects_ObjArray, ref _viewObjectsCount_ObjArray), nameof(_ViewImmutableObjects_ObjArray)) :
                (AddObject(value, ref _ViewImmutableObjects, ref _viewObjectsCount), nameof(_ViewImmutableObjects));
        }

        protected virtual ExpressionSyntax EmitValueReferenceById(int id, string storedField, Type expectedType)
        {
            return
                    SyntaxFactory.ElementAccessExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        UseType(typeof(RoslynValueEmitter)),
                        SyntaxFactory.IdentifierName(storedField)),
                        SyntaxFactory.BracketedArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(EmitValue(id)))));
        }

        private LiteralExpressionSyntax EmitStringLiteral(string value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(int value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(long value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(ulong value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(uint value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(decimal value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(float value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private LiteralExpressionSyntax EmitStandardNumericLiteral(double value)
        {
            return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));
        }

        private ExpressionSyntax EmitStrangeIntegerValue(long value, Type type)
        {
            return SyntaxFactory.CastExpression(this.UseType(type), SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value)));
        }


        public static DotvvmProperty[][] _ViewImmutableObjects_PropArray = new DotvvmProperty[16][];
        public static object[][] _ViewImmutableObjects_ObjArray = new object[16][];
        public static object[] _ViewImmutableObjects = new object[16];
        private static Type[] ImmutableContainers = new [] {
            typeof(ImmutableArray<>), typeof(ImmutableList<>), typeof(ImmutableDictionary<,>), typeof(ImmutableHashSet<>), typeof(ImmutableQueue<>), typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedSet<>), typeof(ImmutableStack<>)
        };
        internal static bool IsImmutableObject(Type t) =>
            typeof(IBinding).IsAssignableFrom(t)
              || t.GetCustomAttribute<HandleAsImmutableObjectInDotvvmPropertyAttribute>() is object
              || t.IsGenericType && ImmutableContainers.Contains(t.GetGenericTypeDefinition()) && t.GenericTypeArguments.All(IsImmutableObject);
        private static int _viewObjectsCount = 0;
        private static int _viewObjectsCount_PropArray = 0;
        private static int _viewObjectsCount_ObjArray = 0;
        private static object viewObjectsLocker = new object();

        protected static int AddObject<T>(T obj, ref T[] storage, ref int counter)
        {
            // Is there any ConcurrentList implementation? feel free to replace this

            var id = Interlocked.Increment(ref counter);
            if (id >= storage.Length)
            {
                lock (viewObjectsLocker)
                {
                    if (id >= storage.Length)
                    {
                        var newArray = new T[storage.Length * 2];
                        Array.Copy(storage, 0, newArray, 0, storage.Length);
                        // read/writes of references are atomic
                        storage = newArray;
                    }
                }
            }

            storage[id] = obj;
            return id;
        }
    }
}
