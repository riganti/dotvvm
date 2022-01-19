using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    /// <summary> Tracks which control properties are used in this markup control and stores that in <see cref="Internal.UsedPropertiesInfoProperty" /> </summary>
    class UsedPropertiesFindingVisitor : ResolvedControlTreeVisitor
    {

        class ExpressionInspectingVisitor: ExpressionVisitor
        {
            public HashSet<DotvvmProperty> UsedProperties { get; } = new();
            public bool UsesViewModel { get; set; }
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value is DotvvmProperty property)
                    UsedProperties.Add(property);
                return base.VisitConstant(node);
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                if (typeof(DotvvmProperty).IsAssignableFrom(node.Type) && node.Member is FieldInfo { IsStatic: true } field)
                    UsedProperties.Add((DotvvmProperty)field.GetValue(null).NotNull());
                return base.VisitMember(node);
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node.GetParameterAnnotation() is { DataContext: { Parent: null } } annotation)
                    UsesViewModel = true;
                return base.VisitParameter(node);
            }
        }

        ExpressionInspectingVisitor exprVisitor = new();

        public override void VisitBinding(ResolvedBinding binding)
        {
            base.VisitBinding(binding);

            var type = binding.BindingType;
            if (typeof(IValueBinding).IsAssignableFrom(type) || typeof(IStaticCommandBinding).IsAssignableFrom(type))
            {
                exprVisitor.Visit(binding.Expression);
            }
        }

        public override void VisitView(ResolvedTreeRoot view)
        {
            if (!typeof(DotvvmMarkupControl).IsAssignableFrom(view.Metadata.Type))
                return;

            base.VisitView(view);

            var props = exprVisitor.UsedProperties.OrderBy(p => p.Name).ToArray();
            var info = new ControlUsedPropertiesInfo(props, exprVisitor.UsesViewModel);

            view.SetProperty(new ResolvedPropertyValue(Internal.UsedPropertiesInfoProperty, info));
        }
    }

    [HandleAsImmutableObjectInDotvvmPropertyAttribute]
    sealed record ControlUsedPropertiesInfo(
        DotvvmProperty[] ClientSideUsedProperties,
        bool UsesViewModelClientSide
    );
}
