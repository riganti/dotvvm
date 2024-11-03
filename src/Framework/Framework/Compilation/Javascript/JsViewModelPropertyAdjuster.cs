using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using FastExpressionCompiler;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JsViewModelPropertyAdjuster : JsNodeVisitor
    {
        private readonly IViewModelSerializationMapper mapper;
        private readonly JsViewModelPropertyAdjuster noStateAdjuster;
        private bool preferUsingState;

        public JsViewModelPropertyAdjuster(
            IViewModelSerializationMapper mapper,
            bool preferUsingState)
        {
            this.mapper = mapper;
            this.preferUsingState = preferUsingState;
            if (preferUsingState)
            {
                this.noStateAdjuster = new JsViewModelPropertyAdjuster(mapper, false);
            }
            else
            {
                this.noStateAdjuster = this;
            }
        }

        protected override void DefaultVisit(JsNode node)
        {
            foreach (var c in node.Children)
            {
                if (c.HasAnnotation(ShouldBeObservableAnnotation.Instance) ||
                    c.Parent is JsAssignmentExpression && c.Role == JsAssignmentExpression.LeftRole)
                {
                    // This method or assignment expects observable, so we stop prefering state for the subtree
                    c.AcceptVisitor(noStateAdjuster);
                }
                else
                {
                    c.AcceptVisitor(this);
                }
            }

            if (node.Annotation<VMPropertyInfoAnnotation>() is { MemberInfo: var member } propAnnotation)
            {
                var target = propAnnotation.EvaluateTargetPath(node);

                if (target is {} && target.HasAnnotation(ObservableUnwrapInvocationAnnotation.Instance))
                    target = target.GetChildByRole(JsTreeRoles.TargetExpression);
                else if (target is {} && target.HasAnnotation(ObservableSetterInvocationAnnotation.Instance))
                    throw new NotImplementedException();

                var propertyType = propAnnotation.ResultType;
                var annotation = node.Annotation<ViewModelInfoAnnotation>() ?? new ViewModelInfoAnnotation(propertyType);

                var targetAnnotation = target?.Annotation<ViewModelInfoAnnotation>();
                var propertyIsObservable = propAnnotation.IsObservable;
                if (targetAnnotation is {})
                {
                    propAnnotation.SerializationMap ??=
                        targetAnnotation.SerializationMap?.Properties
                        .FirstOrDefault(p => p.PropertyInfo == member);
                    annotation.ObservableMap = targetAnnotation.ObservableMap.GetChildObject(propAnnotation.ObjectPath.AsSpan()).OverrideWith(annotation.ObservableMap);
                    propertyIsObservable ??= targetAnnotation.ObservableMap.IsPropertyObservable(propAnnotation.ObjectPath.AsSpan());
                }
                if (propAnnotation.SerializationMap is ViewModelPropertyMap propertyMap)
                {
                    if (propertyMap.ViewModelProtection == ViewModel.ProtectMode.EncryptData) throw new Exception($"Property {member?.Name} is encrypted and cannot be used in JS.");
                    if (node is JsMemberAccessExpression memberAccess && propertyMap.Name != memberAccess.MemberName)
                    {
                        memberAccess.MemberName = propertyMap.Name;
                    }
                }
                else if (member is FieldInfo)
                    throw new NotSupportedException($"Cannot translate field '{member}' to Javascript");

                if (targetAnnotation is { IsControl: true } &&
                    member is {} &&
                    typeof(DotvvmBindableObject).IsAssignableFrom(member.DeclaringType) &&
                    DotvvmProperty.ResolveProperty(member.DeclaringType, member.Name) is null &&
                    DotvvmPropertyGroup.ResolvePropertyGroup(member.DeclaringType, member.Name) is null)
                {
                    // Plain .NET is used on _control, this property is not serialized, so it will not work client-side
                    throw new NotSupportedException($"Control property {member.Name} is not a registered DotvvmProperty and cannot be used client-side. Either use a resource binding to use the server-side value, or see https://www.dotvvm.com/docs/latest/pages/concepts/control-development/control-properties how to register a DotvvmProperty.");
                }

                if (member is {} && typeof(IDotvvmPrimitiveType).IsAssignableFrom(member.DeclaringType))
                {
                    throw new NotSupportedException($"Cannot translate property {member.Name} on custom primitive type {member.DeclaringType.ToCode()} to Javascript. Use the ToString() method to get the underlying value.");
                }

                if (propertyIsObservable ?? !preferUsingState) // if we don't know -> assume what is the current preference
                {
                    node.AddAnnotation(ResultIsObservableAnnotation.Instance);

                    if (ReflectionUtils.IsCollection(propertyType))
                    {
                        node.AddAnnotation(ResultIsObservableArrayAnnotation.Instance);
                    }
                }

                node.WithAnnotation(annotation, append: false);
                node.AddAnnotation(MayBeNullAnnotation.Instance);
            }

            if (node.Annotation<ViewModelInfoAnnotation>() is { Type: {}, SerializationMap: null } vmAnnotation)
            {
                vmAnnotation.SerializationMap = mapper.GetMap(vmAnnotation.Type);
            }
        }

        public override void VisitMemberAccessExpression(JsMemberAccessExpression expr)
        {
            // replace $data.Property with $data.Property.state
            if (preferUsingState && expr.Target is JsSymbolicParameter { Symbol: JavascriptTranslator.ViewModelSymbolicParameter vmSymbol })
            {
                var propertyAnnotation = expr.Annotation<VMPropertyInfoAnnotation>();
                var typeAnnotation = expr.Annotation<ViewModelInfoAnnotation>() ?? new ViewModelInfoAnnotation(propertyAnnotation!.MemberInfo!.GetResultType());
                expr = (JsMemberAccessExpression)expr.ReplaceWith(_ =>
                    expr.WithAnnotation(ShouldBeObservableAnnotation.Instance)
                        .Member("state")
                        .WithAnnotation(new ViewModelInfoAnnotation(typeAnnotation.Type, typeAnnotation.IsControl, typeAnnotation.ExtensionParameter, containsObservables: false))
                        .WithConditionalAnnotation(expr.HasAnnotation(MayBeNullAnnotation.Instance), MayBeNullAnnotation.Instance)
                );
            }
            DefaultVisit(expr);

            // by some luck, we got an observable into the tree -> replace the property with Prop.state to get rid of it
            if (preferUsingState && expr.HasAnnotation(ResultIsObservableAnnotation.Instance))
            {
                var typeAnnotation = expr.Annotation<ViewModelInfoAnnotation>().NotNull();
                expr.ReplaceWith(_ =>
                    expr.WithAnnotation(ShouldBeObservableAnnotation.Instance)
                        .Member("state")
                        .WithAnnotation(new ViewModelInfoAnnotation(typeAnnotation.Type, typeAnnotation.IsControl, typeAnnotation.ExtensionParameter, containsObservables: false))
                        .WithConditionalAnnotation(expr.HasAnnotation(MayBeNullAnnotation.Instance), MayBeNullAnnotation.Instance)
                );
            }
        }

        public override void VisitSymbolicParameter(JsSymbolicParameter expr)
        {
            Debug.Assert(!expr.Children.Any());

            JsExpression newExpr = expr;

            if (expr.Symbol is JavascriptTranslator.ViewModelSymbolicParameter vmSymbol)
            {
                // When we see reference to viewModel (like $data, $parent, ...), we replace it with reference to the state
                var vmType = expr.Annotation<ViewModelInfoAnnotation>().NotNull("viewmodel must have type annotation (ViewModelInfoAnnotation)");
                if (preferUsingState)
                {
                    vmType.ContainsObservables = false;
                    newExpr = (JsExpression)expr.ReplaceWith(_ =>
                        JavascriptTranslator.GetKnockoutViewModelParameter(vmSymbol.ParentIndex, returnsObservable: true).ToExpression()
                            .Member("state").WithAnnotation(vmType));
                }
                else
                {
                    vmType.ContainsObservables = true;
                }
            }

            DefaultVisit(expr);
        }
    }

    /// <summary> Indicates that this invocation only unwraps a knockout observable and was automatically inserted by <see cref="KnockoutObservableHandlingVisitor" /> </summary>
    public sealed class ObservableUnwrapInvocationAnnotation
    {
        private ObservableUnwrapInvocationAnnotation() { }
        public static ObservableUnwrapInvocationAnnotation Instance = new ObservableUnwrapInvocationAnnotation();
    }
    /// <summary> Indicates that this invocation is a knockout observable value assignment and was automatically inserted by <see cref="KnockoutObservableHandlingVisitor" /> </summary>
    public sealed class ObservableSetterInvocationAnnotation
    {
        private ObservableSetterInvocationAnnotation() { }
        public static ObservableSetterInvocationAnnotation Instance = new ObservableSetterInvocationAnnotation();
    }
    /// <summary> Result of the annotated expression is always a knockout observable. DotVVM will automatically unwrap the observable, unless the <see cref="ShouldBeObservableAnnotation" /> is also specified. </summary>
    public sealed class ResultIsObservableAnnotation
    {
        private ResultIsObservableAnnotation() { }
        public static ResultIsObservableAnnotation Instance = new ResultIsObservableAnnotation();
    }
    /// <summary> Result is a knockout observable array. </summary>
    public sealed class ResultIsObservableArrayAnnotation
    {
        private ResultIsObservableArrayAnnotation() { }
        public static ResultIsObservableArrayAnnotation Instance = new ResultIsObservableArrayAnnotation();
    }
    /// <summary> Result of the annotated expression may or may not be a knockout observable. DotVVM will automatically unwrap the observable, unless the <see cref="ShouldBeObservableAnnotation" /> is also specified. </summary>
    public sealed class ResultMayBeObservableAnnotation
    {
        private ResultMayBeObservableAnnotation() { }
        public static ResultMayBeObservableAnnotation Instance = new ResultMayBeObservableAnnotation();
    }
    /// <summary> This node should return observable -  instructs the <see cref="KnockoutObservableHandlingVisitor" /> to not unwrap the node and the <see cref="JsViewModelPropertyAdjuster" /> to not use .state in the subexpression. </summary>
    public sealed class ShouldBeObservableAnnotation
    {
        private ShouldBeObservableAnnotation() { }
        public static ShouldBeObservableAnnotation Instance = new ShouldBeObservableAnnotation();
    }
    /// <summary> Instruct the <see cref="KnockoutObservableHandlingVisitor" /> to process the node after it's children are resolved and before it is handled itself by the rules </summary>
    public sealed class ObservableTransformationAnnotation
    {
        public readonly Func<JsExpression, JsExpression> TransformExpression;
        public ObservableTransformationAnnotation(Func<JsExpression, JsExpression> transformExpression)
        {
            TransformExpression = transformExpression;
        }

        /// <summary> Makes sure that the observable is fully wrapped in observable (i.e. wraps the expression in `ko.pureComputed(...)` when needed) </summary>
        public static readonly ObservableTransformationAnnotation EnsureWrapped = new ObservableTransformationAnnotation(JsAstHelpers.EnsureObservableWrapped);
    }
}
