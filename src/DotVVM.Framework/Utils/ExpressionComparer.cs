#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DotVVM.Framework.Utils
{
    public class ExpressionComparer : IEqualityComparer<Expression>, IEqualityComparer<MemberInfo>
    {
        public static readonly ExpressionComparer Instance = new ExpressionComparer();

        public ExpressionComparer(bool cacheHashValues = false)
        {
            if (cacheHashValues) CacheHashValues();
        }

        public static bool Equals(MemberInfo a, MemberInfo b)
        {
            if (a == b) return true;
            if (a == null || b == null) return false;
            if (a.MetadataToken != b.MetadataToken) return false;
            if (a.DeclaringType!.Assembly == b.DeclaringType!.Assembly) return true;
            throw new NotImplementedException();
        }

        public static int GetHashCode(MemberInfo mi)
        {
            return mi.MetadataToken * 17 + (mi.DeclaringType?.GetTypeInfo()?.MetadataToken ?? 0);
        }

        bool IEqualityComparer<MemberInfo>.Equals(MemberInfo a, MemberInfo b) => Equals(a, b);
        int IEqualityComparer<MemberInfo>.GetHashCode(MemberInfo a) => GetHashCode(a);


        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public bool Equals(Expression x, Expression y)
        {
            if ((object)x == (object)y) return true;
            if (x == null || y == null) return false;
            var type = x.NodeType;
            if (y.NodeType != type) return false;
            if (x.Type != y.Type) return false;
            if (HashCache != null && HashCache.TryGetValue(x, out var hashx) && HashCache.TryGetValue(x, out var hashy) && hashy.Item1 != hashx.Item1) return false;

            switch (type)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.And:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.LeftShift:
                case ExpressionType.OnesComplement:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Coalesce:
                    var binX = (BinaryExpression)x;
                    var binY = (BinaryExpression)y;
                    return Equals(binX.Method, binY.Method) && Equals(binX.Left, binY.Left) && Equals(binX.Right, binY.Right);
                case ExpressionType.Call:
                    var callX = (MethodCallExpression)x;
                    var callY = (MethodCallExpression)y;
                    return Equals(callX.Method, callY.Method) && Equals(callX.Object, callY.Object) && callX.Arguments.Zip(callY.Arguments, Equals).All(f => f);
                case ExpressionType.ConvertChecked:
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Quote:
                case ExpressionType.Not:
                case ExpressionType.TypeAs:
                case ExpressionType.NegateChecked:
                case ExpressionType.Unbox:
                case ExpressionType.Convert:
                case ExpressionType.ArrayLength:
                    var unX = (UnaryExpression)x;
                    var unY = (UnaryExpression)y;
                    return Equals(unX.Method, unY.Method) && Equals(unX.Operand, unY.Operand);
                case ExpressionType.Conditional:
                    var condX = (ConditionalExpression)x;
                    var condY = (ConditionalExpression)y;
                    return Equals(condX.IfFalse, condY.IfFalse) && Equals(condX.IfTrue, condY.IfTrue) && Equals(condX.Test, condY.Test);
                case ExpressionType.Constant:
                    var constX = (ConstantExpression)x;
                    var constY = (ConstantExpression)y;
                    return constX.Value == constY.Value || constX.Value.Equals(constY.Value);
                case ExpressionType.Invoke:
                    var invX = (InvocationExpression)x;
                    var invY = (InvocationExpression)y;
                    return Equals(invX.Expression, invY.Expression) && invX.Arguments.Zip(invY.Arguments, Equals).All(f => f);
                case ExpressionType.Lambda:
                    var lamX = (LambdaExpression)x;
                    var lamY = (LambdaExpression)y;
                    return lamX.Name == lamY.Name && lamX.Parameters.Count == lamY.Parameters.Count && lamX.Parameters.Zip(lamY.Parameters, Equals).All(f => f) && Equals(lamX.Body, lamY.Body);
                case ExpressionType.ListInit:
                    var linitX = (ListInitExpression)x;
                    var linitY = (ListInitExpression)y;
                    return Equals(linitX.NewExpression, linitY.NewExpression) && linitX.Initializers.Zip(linitY.Initializers, Equals).All(f => f);
                case ExpressionType.MemberAccess:
                    var memX = (MemberExpression)x;
                    var memY = (MemberExpression)y;
                    return Equals(memX.Member, memY.Member) && Equals(memX.Expression, memY.Expression);
                case ExpressionType.MemberInit:
                    var minitX = (MemberInitExpression)x;
                    var minitY = (MemberInitExpression)y;
                    throw new NotImplementedException();
                //return Equals(minitX, minitY) && minitX.Bindings.Zip(minitY.Bindings, (a, b) => a.BindingType == b.BindingType && a.Member == b.Member).All(f => f);
                case ExpressionType.New:
                    var newX = (NewExpression)x;
                    var newY = (NewExpression)y;
                    return Equals(newX.Constructor, newY.Constructor) && newX.Arguments.Zip(newY.Arguments, Equals).All(f => f);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    var newArrX = (NewArrayExpression)x;
                    var newArrY = (NewArrayExpression)y;
                    return newArrX.Expressions.Zip(newArrY.Expressions, Equals).All(f => f);
                case ExpressionType.Parameter:
                    var paramX = (ParameterExpression)x;
                    var paramY = (ParameterExpression)y;
                    return paramX.Name == paramY.Name && paramX.IsByRef == paramY.IsByRef && paramX.Name != null && !paramX.Name.StartsWith("__");
                case ExpressionType.TypeEqual:
                case ExpressionType.TypeIs:
                    var typeBinX = (TypeBinaryExpression)x;
                    var typeBinY = (TypeBinaryExpression)y;
                    return typeBinX.TypeOperand == typeBinY.TypeOperand && Equals(typeBinX.Expression, typeBinY.Expression);
                case ExpressionType.Default:
                    return true;
                case ExpressionType.Block:
                    var blockX = (BlockExpression)x;
                    var blockY = (BlockExpression)y;
                    return blockX.Variables.SequenceEqual(blockY.Variables, this) &&
                        blockY.Expressions.SequenceEqual(blockY.Expressions, this);
                case ExpressionType.Index:
                    var indexX = (IndexExpression)x;
                    var indexY = (IndexExpression)y;
                    return Equals(indexX.Indexer, indexY.Indexer) && indexX.Arguments.SequenceEqual(indexY.Arguments, this) &&
                        Equals(indexX.Object, indexY.Object);
                case ExpressionType.Extension:
                    //Debug.Assert(x is MyParameterExpression);
                    return x.Equals(y);
                default:
                    throw new NotImplementedException($"Comparing expression of type { type} is not supported.");
            }
        }

        System.Runtime.CompilerServices.ConditionalWeakTable<Expression, Tuple<int>>? HashCache { get; set; }

        public void CacheHashValues()
        {
            if (HashCache == null) HashCache = new System.Runtime.CompilerServices.ConditionalWeakTable<Expression, Tuple<int>>();
        }

        int GetHashCode(IEnumerable<Expression> expressions)
        {
            int hash = 368814364;
            foreach (var item in expressions)
            {
                hash += GetHashCode(item);
                hash *= 17;
            }
            return hash;
        }
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public int GetHashCode(Expression obj)
        {
            unchecked
            {
                if (obj == null) return 0x77777777;
                if (HashCache != null)
                {
                    if (HashCache.TryGetValue(obj, out var chash)) return chash.Item1;
                }
                var type = obj.NodeType;
                var hash = 0x555555555;
                hash += (int)type * 23;
                hash += obj.Type.GetHashCode();
                switch (type)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.And:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Power:
                    case ExpressionType.RightShift:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.LeftShift:
                    case ExpressionType.OnesComplement:
                    case ExpressionType.Assign:
                    case ExpressionType.AddAssign:
                    case ExpressionType.AndAssign:
                    case ExpressionType.DivideAssign:
                    case ExpressionType.ExclusiveOrAssign:
                    case ExpressionType.LeftShiftAssign:
                    case ExpressionType.ModuloAssign:
                    case ExpressionType.MultiplyAssign:
                    case ExpressionType.OrAssign:
                    case ExpressionType.PowerAssign:
                    case ExpressionType.RightShiftAssign:
                    case ExpressionType.SubtractAssign:
                    case ExpressionType.AddAssignChecked:
                    case ExpressionType.MultiplyAssignChecked:
                    case ExpressionType.SubtractAssignChecked:
                    case ExpressionType.PreIncrementAssign:
                    case ExpressionType.PreDecrementAssign:
                    case ExpressionType.PostIncrementAssign:
                    case ExpressionType.PostDecrementAssign:
                    case ExpressionType.IsTrue:
                    case ExpressionType.IsFalse:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Coalesce:
                        var bin = (BinaryExpression)obj;
                        hash *= 17;
                        hash += GetHashCode(bin.Left);
                        hash *= 19;
                        hash += GetHashCode(bin.Right);
                        break;
                    case ExpressionType.Call:
                        var call = (MethodCallExpression)obj;
                        hash *= 19;
                        hash += GetHashCode(call.Method);
                        hash *= 23;
                        hash += GetHashCode(call.Object);
                        foreach (var arg in call.Arguments)
                        {
                            hash *= 17;
                            hash += GetHashCode(arg);
                        }
                        break;
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.Negate:
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.Quote:
                    case ExpressionType.Not:
                    case ExpressionType.TypeAs:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Unbox:
                    case ExpressionType.Convert:
                    case ExpressionType.ArrayLength:
                        var un = (UnaryExpression)obj;
                        hash *= 29;
                        hash += GetHashCode(un.Operand);
                        if (un.Method != null)
                        {
                            hash *= 17;
                            hash += GetHashCode(un.Method);
                        }
                        break;
                    case ExpressionType.Conditional:
                        var cond = (ConditionalExpression)obj;
                        hash *= 17;
                        hash += GetHashCode(cond.IfFalse);
                        hash *= 23;
                        hash += GetHashCode(cond.IfTrue);
                        hash *= 19;
                        hash += GetHashCode(cond.Test);
                        break;
                    case ExpressionType.Constant:
                        var conste = (ConstantExpression)obj;
                        hash *= 17;
                        hash += conste.Value?.GetHashCode() ?? 6446;
                        break;
                    case ExpressionType.Invoke:
                        var inv = (InvocationExpression)obj;
                        hash *= 17;
                        hash += GetHashCode(inv.Expression);
                        foreach (var arg in inv.Arguments)
                        {
                            hash *= 19;
                            hash += GetHashCode(arg);
                        }
                        break;
                    case ExpressionType.Lambda:
                        var lam = (LambdaExpression)obj;
                        foreach (var p in lam.Parameters)
                        {
                            hash *= 17;
                            hash += GetHashCode(p);
                        }
                        hash *= 23;
                        hash += lam.Name?.GetHashCode() ?? 3215;
                        hash *= 19;
                        hash += GetHashCode(lam.Body);
                        break;
                    case ExpressionType.ListInit:
                        var linit = (ListInitExpression)obj;
                        hash *= 17;
                        hash += GetHashCode(linit.NewExpression);
                        foreach (var ini in linit.Initializers)
                        {
                            hash *= 23;
                            hash += GetHashCode(ini.AddMethod);
                            hash *= 29;
                            hash += GetHashCode(ini.Arguments);
                        }
                        break;
                    case ExpressionType.MemberAccess:
                        var mem = (MemberExpression)obj;
                        hash *= 23;
                        hash += GetHashCode(mem.Member);
                        hash *= 19;
                        hash += GetHashCode(mem.Expression);
                        break;
                    case ExpressionType.MemberInit:
                        var minit = (MemberInitExpression)obj;
                        throw new NotImplementedException();
                    //return Equals(minitX, minitY) && minitX.Bindings.Zip(minitY.Bindings, (a, b) => a.BindingType == b.BindingType && a.Member == b.Member).All(f => f);
                    case ExpressionType.New:
                        var newe = (NewExpression)obj;
                        hash *= 31;
                        hash += GetHashCode(newe.Constructor);
                        foreach (var arg in newe.Arguments)
                        {
                            hash *= 29;
                            hash += GetHashCode(arg);
                        }
                        break;
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                        var newArr = (NewArrayExpression)obj;
                        foreach (var arg in newArr.Expressions)
                        {
                            hash *= 37;
                            hash += GetHashCode(arg);
                        }
                        break;
                    case ExpressionType.Parameter:
                        var param = (ParameterExpression)obj;
                        hash *= 17;
                        if (param.Name != null && !param.Name.StartsWith("__"))
                        {
                            hash += param.Name.GetHashCode();
                        }
                        else
                        {
                            hash += param.GetHashCode();
                        }
                        if (param.IsByRef) hash++;
                        break;
                    case ExpressionType.TypeEqual:
                    case ExpressionType.TypeIs:
                        var typeBin = (TypeBinaryExpression)obj;
                        hash *= 29;
                        hash += typeBin.TypeOperand.GetHashCode();
                        hash *= 17;
                        hash += GetHashCode(typeBin.Expression);
                        break;
                    case ExpressionType.Default:
                        hash += 6546546546;
                        break;
                    case ExpressionType.Block:
                        var blockExpression = (BlockExpression)obj;
                        hash *= 31;
                        hash += GetHashCode(blockExpression.Variables);
                        hash *= 3;
                        hash += GetHashCode(blockExpression.Expressions);
                        break;
                    case ExpressionType.Switch:
                        var switchExpr = (SwitchExpression)obj;
                        hash += GetHashCode(switchExpr.DefaultBody);
                        hash *= 31;
                        hash += GetHashCode(switchExpr.SwitchValue);
                        foreach (var expr in switchExpr.Cases)
                        {
                            hash *= 7;
                            hash += GetHashCode(expr.Body);
                            hash += 11;
                            hash += GetHashCode(expr.TestValues);
                        }
                        break;
                    case ExpressionType.Extension:
                        hash += obj.ToString().GetHashCode(); break;
                    case ExpressionType.Index:
                        var indexExpr = (IndexExpression)obj;
                        hash += GetHashCode(indexExpr.Arguments);
                        hash += indexExpr.Indexer?.GetHashCode() ?? 6453124768463154687;
                        hash *= 47;
                        hash += GetHashCode(indexExpr.Object);
                        break;
                    default:
                        throw new NotImplementedException($"GetHasCode of expression type { type } is not supported.");
                }
                hash *= 4611686018427387847;

                if (HashCache != null) HashCache.Add(obj, Tuple.Create((int)hash));

                return (int)hash;
            }
        }
    }
}
