using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
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

                if (propAnnotation.SerializationMap == null && target?.Annotation<ViewModelInfoAnnotation>() is ViewModelInfoAnnotation targetAnnotation)
                {
                    var property = propAnnotation.SerializationMap = targetAnnotation.SerializationMap.Properties.First(p => p.PropertyInfo == propAnnotation.MemberInfo);
                    if (property == null) throw new Exception($"Unable to find property {propAnnotation.MemberInfo.Name} is serialization map.");
                }
                if (propAnnotation.SerializationMap is ViewModelPropertyMap propertyMap)
                {
                    if (propertyMap.ViewModelProtection == ViewModel.ProtectMode.EncryptData) throw new Exception($"Property {propAnnotation.MemberInfo.Name} is encrypted and cannot be used in JS.");
                    if (node is JsMemberAccessExpression memberAccess && propertyMap.Name != memberAccess.MemberName)
                    {
                        memberAccess.MemberName = propertyMap.Name;
                    }

                    node.AddAnnotation(ResultIsObservableAnnotation.Instance);
                    node.AddAnnotation(new ViewModelInfoAnnotation(propertyMap.Type));

                    // handle observable
                    if (node.Role == JsAssignmentExpression.LeftRole && node.Parent is JsAssignmentExpression parentAssignment)
                    {
                        if (propertyMap.Populate)
                            parentAssignment.ReplaceWith(_ => new JsIdentifierExpression("dotvvm").Member("serialization").Member("deserialize").Invoke(parentAssignment.Right, parentAssignment.Left));
                        else parentAssignment.ReplaceWith(_ => new JsInvocationExpression(parentAssignment.Left, parentAssignment.Right)
                            .WithAnnotation(ObservableSetterInvocationAnnotation.Instance));
                    }
                    else if (node.Parent is JsExpression parent)
                    {
                        node.ReplaceWith(_ => ((JsExpression)node).Invoke().WithAnnotation(ObservableUnwrapInvocationAnnotation.Instance));
                    }
                }
                else throw new Exception();
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
}
