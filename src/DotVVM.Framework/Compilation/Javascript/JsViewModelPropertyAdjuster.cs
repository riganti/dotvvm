using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JsViewModelPropertyAdjuster : JsNodeVisitor
    {
        private readonly IViewModelSerializationMapper mapper;

        public JsViewModelPropertyAdjuster(IViewModelSerializationMapper mapper)
        {
            this.mapper = mapper;
        }

        protected override void DefaultVisit(JsNode node)
        {
            base.DefaultVisit(node);

            if (node.Annotation<VMPropertyInfoAnnotation>() is VMPropertyInfoAnnotation propAnnotation)
            {
                var target = node.GetChildByRole(JsTreeRoles.TargetExpression);
                if (target.HasAnnotation<ObservableUnwrapInvocationAnnotation>())
                    target = target.GetChildByRole(JsTreeRoles.TargetExpression);
                else if (target.HasAnnotation<ObservableSetterInvocationAnnotation>())
                    throw new NotImplementedException();

                var propertyType = propAnnotation.MemberInfo.GetResultType();
                var containsObservables = true;
                if (propAnnotation.SerializationMap == null && target?.Annotation<ViewModelInfoAnnotation>() is ViewModelInfoAnnotation targetAnnotation)
                {
                    propAnnotation.SerializationMap = targetAnnotation.SerializationMap?.Properties.FirstOrDefault(p => p.PropertyInfo == propAnnotation.MemberInfo);
                    containsObservables = targetAnnotation.ContainsObservables;
                }
                if (propAnnotation.SerializationMap is ViewModelPropertyMap propertyMap)
                {
                    if (propertyMap.ViewModelProtection == ViewModel.ProtectMode.EncryptData) throw new Exception($"Property {propAnnotation.MemberInfo.Name} is encrypted and cannot be used in JS.");
                    if (node is JsMemberAccessExpression memberAccess && propertyMap.Name != memberAccess.MemberName)
                    {
                        memberAccess.MemberName = propertyMap.Name;
                    }
                }
                else if (propAnnotation.MemberInfo is FieldInfo)
                    throw new NotSupportedException($"Can not translate field '{propAnnotation.MemberInfo}' to Javascript");

                if (containsObservables)
                {
                    node.AddAnnotation(ResultIsObservableAnnotation.Instance);

                    if (ReflectionUtils.IsCollection(propertyType))
                    {
                        node.AddAnnotation(ResultIsObservableArrayAnnotation.Instance);
                    }
                }

                node.AddAnnotation(new ViewModelInfoAnnotation(propertyType, containsObservables: containsObservables));
                node.AddAnnotation(MayBeNullAnnotation.Instance);
            }

            if (node.Annotation<ViewModelInfoAnnotation>() is var vmAnnotation && vmAnnotation?.Type != null && vmAnnotation.SerializationMap == null)
            {
                vmAnnotation.SerializationMap = mapper.GetMap(vmAnnotation.Type);
            }
        }
    }

    public sealed class ObservableUnwrapInvocationAnnotation
    {
        private ObservableUnwrapInvocationAnnotation() { }
        public static ObservableUnwrapInvocationAnnotation Instance = new ObservableUnwrapInvocationAnnotation();
    }
    public sealed class ObservableSetterInvocationAnnotation
    {
        private ObservableSetterInvocationAnnotation() { }
        public static ObservableSetterInvocationAnnotation Instance = new ObservableSetterInvocationAnnotation();
    }
    public sealed class ResultIsObservableAnnotation
    {
        private ResultIsObservableAnnotation() { }
        public static ResultIsObservableAnnotation Instance = new ResultIsObservableAnnotation();
    }
    public sealed class ResultIsObservableArrayAnnotation
    {
        private ResultIsObservableArrayAnnotation() { }
        public static ResultIsObservableArrayAnnotation Instance = new ResultIsObservableArrayAnnotation();
    }
    public sealed class ResultMayBeObservableAnnotation
    {
        private ResultMayBeObservableAnnotation() { }
        public static ResultMayBeObservableAnnotation Instance = new ResultMayBeObservableAnnotation();
    }
    public sealed class ShouldBeObservableAnnotation
    {
        private ShouldBeObservableAnnotation() { }
        public static ShouldBeObservableAnnotation Instance = new ShouldBeObservableAnnotation();
    }
    /// Instruct the <see cref="KnockoutObservableHandlingVisitor" /> to process the node after it's children are resolved and before it is handled itself by the rules
    public sealed class ObservableTransformationAnnotation
    {
        public readonly Func<JsExpression, JsExpression> TransformExpression;
        public ObservableTransformationAnnotation(Func<JsExpression, JsExpression> transformExpression)
        {
            TransformExpression = transformExpression;
        }

        /// Makes sure that the observable is fully wrapped in observable (i.e. wraps the expression in `ko.pureComputed(...)` when needed)
        public static readonly ObservableTransformationAnnotation EnsureWrapped = new ObservableTransformationAnnotation(JsAstHelpers.EnsureObservableWrapped);
    }
}
