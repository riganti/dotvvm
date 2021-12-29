using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{

using static DotvvmPropertyUtils.ReferenceType;
public static class DotvvmPropertyUtils
{
    public static DotvvmProperty GetDotvvmPropertyFromExpression(LambdaExpression lambda) =>
        GetDotvvmPropertyFromExpression(lambda.Body);
    public static DotvvmProperty GetDotvvmPropertyFromExpression(Expression expression)
    {
        var (t, p) = ParsePropReference(expression);
        if (t == Property)
            return p!;
        else
            throw new NotSupportedException($"Expression {expression} of type {expression.NodeType} is not supported.");
    }

    internal enum ReferenceType { This, Property }

    static (ReferenceType type, DotvvmProperty? property) ParsePropReference(Expression expression)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.Convert:
                // ignore conversions
                return ParsePropReference(((UnaryExpression)expression).Operand);
            case ExpressionType.Parameter:
                return (This, null);
            case ExpressionType.MemberAccess: {
                var me = (MemberExpression)expression;
                if (me.Expression is null)
                    goto default;
                var prop = me.Member;
                // just skip ValueOrBinding unwrapping
                if (prop.Name is nameof(ValueOrBinding<int>.ValueOrDefault) or nameof(ValueOrBinding<int>.BindingOrDefault) && 
                    prop.DeclaringType!.IsGenericType && prop.DeclaringType.GetGenericTypeDefinition() == typeof(ValueOrBinding<>))
                    return ParsePropReference(me.Expression);
                return ParseProperty(me.Expression, prop);
            }
            case ExpressionType.Index: {
                // this may be property group access .SetProperty(a => a.Attributes["class"], class)
                var ie = (IndexExpression)expression;
                return ParseIndexer(ie.Object, ie.Arguments.Single());
            }
            case ExpressionType.Call: {
                var ce = (MethodCallExpression)expression;
                var m = ce.Method;
                // control.GetCapability<HtmlCapability>()
                if (m.DeclaringType == typeof(DotvvmBindableObjectHelper) && m.Name == "GetCapability")
                    return ParseGetCapability(ce);
                else if (m.Name == "get_Item" && m.GetParameters().Length == 1)
                    return ParseIndexer(ce.Object, ce.Arguments[0]);

                goto default;
            }
            case ExpressionType.New: {
                var ne = (NewExpression)expression;

                // skip new ValueOrBinding(myProperty)
                if (ne.Constructor!.DeclaringType!.IsGenericType && ne.Constructor.DeclaringType.GetGenericTypeDefinition() == typeof(ValueOrBinding<>))
                    return ParsePropReference(ne.Arguments.Single());

                goto default;
            }
            default:
                throw new NotSupportedException($"Expression {expression} is not supported.");
        }
    }

    static (ReferenceType, DotvvmProperty?) ParseGetCapability(MethodCallExpression ce)
    {
        var targetExpr = ce.Arguments.First();
        var target = ParsePropReference(targetExpr);
        if (target.type != This)
            throw new Exception($"Can not get capability from {ce.Object}");
        var capabilityType = ce.Method.GetGenericArguments().Single();
        var prefix = ce.Arguments.ElementAtOrDefault(1)?.Apply(GetConstant<string>);
        var capprop = DotvvmCapabilityProperty.Find(targetExpr.Type, capabilityType, prefix);
        return (Property, capprop);
    }

    static (ReferenceType, DotvvmProperty?) ParseProperty(Expression targetExpr, MemberInfo prop)
    {
        var target = ParsePropReference(targetExpr);
        if (target.type == This)
        {
            // normal dotvvm property
            var dotprop =
                DotvvmProperty.ResolveProperty(prop.DeclaringType!, prop.Name) ??
                throw new Exception($"Property '{prop.DeclaringType!.Name}.{prop.Name}' is not a registered DotvvmProperty.");
            return (Property, dotprop);
        }
        else if (target.type == Property)
        {
            // dotvvm property inside a capability
            if (target.property is not DotvvmCapabilityProperty capprop)
                throw new NotSupportedException($"Can not access property on another dotvvm property: {targetExpr}");
            var mapping = capprop.PropertyMapping ??
                throw new Exception($"Capability property {capprop} does not have a property mapping, thus can not be used in this helper method.");
            var p = mapping.SingleOrDefault(p => p.prop == prop);
            if (p.dotvvmProperty is null)
                throw new Exception($"Capability property {capprop} does not contain property {prop.Name}.");
            return (Property, p.dotvvmProperty);
        }
        else throw null!;
    }

    static (ReferenceType, DotvvmProperty) ParseIndexer(Expression? targetExpr, Expression index)
    {
        var me = targetExpr as MemberExpression ?? throw new NotSupportedException($"Can not parse property from {targetExpr}[{index}]");
        if (me.Expression is null)
            throw new NotSupportedException($"Expression {targetExpr} is not supported, accesssing static properties isn't allowed.");
        var prop = me.Member;
        var name = GetConstant<string>(index);
        var target = ParsePropReference(me.Expression);
        if (target.type == This)
        {
            // normal property group
            var pgroup =
                DotvvmPropertyGroup.ResolvePropertyGroup(prop.DeclaringType!, prop.Name) ??
                throw new Exception($"'{prop.DeclaringType!.Name}.{prop.Name}' is not a registered property group.");
            return (Property, pgroup.GetDotvvmProperty(name));
        }
        else if (target.type == Property)
        {
            // property group inside a capability
            if (target.property is not DotvvmCapabilityProperty capprop)
                throw new Exception($"Can not access property group on another dotvvm property: {targetExpr}");
            var mapping = capprop.PropertyGroupMapping ??
                throw new Exception($"Capability property {capprop} does not have a property group mapping, thus can not be used in this helper method.");
            var p = mapping.SingleOrDefault(p => p.prop == prop);
            if (p.dotvvmPropertyGroup is null)
                throw new Exception($"Capability property {capprop} does not contain property group {prop.Name}.");
            return (Property, p.dotvvmPropertyGroup.GetDotvvmProperty(name));
        }
        else throw null!;
    }

    /// <summary> Gets a constant from the expression or throws if it's not possible </summary>
    static T GetConstant<T>(Expression expression)
    {
        while (expression.NodeType == ExpressionType.Convert)
            expression = ((UnaryExpression)expression).Operand;

        if (expression.NodeType != ExpressionType.Constant)
            expression = expression.OptimizeConstants();

        if (expression.NodeType == ExpressionType.Constant)
            return ((ConstantExpression)expression).Value switch {
                T x => x,
                null => default!,
                var x => throw new NotSupportedException($"{expression} was expected to be of type {typeof(T).Name}")
            };
        throw new NotSupportedException($"Cannot get constant from {expression}");
    }
}
}
