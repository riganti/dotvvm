using System;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using System.Linq.Expressions;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Tests.Runtime.ControlTree.DefaultControlTreeResolver
{
    [DataContextChanger]
    public class ControlWithSpecialBindingsInside : DotvvmControl
    {
        public class DataContextChanger : DataContextStackManipulationAttribute
        {
            public override IDataContextStack ChangeStackForChildren(IDataContextStack original, IAbstractControl control, IPropertyDescriptor property, Func<IDataContextStack, ITypeDescriptor, IDataContextStack> createNewFrame)
            {
                return DataContextStack.Create(ResolvedTypeDescriptor.ToSystemType(original.DataContextType), (DataContextStack)original.Parent,
                    bindingPropertyResolvers: new Delegate[]{
                        new Func<ParsedExpressionBindingProperty, ParsedExpressionBindingProperty>(e => {
                            if (e.Expression.NodeType == ExpressionType.Constant && (string)((ConstantExpression)e.Expression).Value == "abc") return new ParsedExpressionBindingProperty(Expression.Constant("def"));
                            else return e;
                        })
                    });
            }

            public override DataContextStack ChangeStackForChildren(DataContextStack original, DotvvmBindableObject obj, DotvvmProperty property, Func<DataContextStack, Type, DataContextStack> createNewFrame)
            {
                return DataContextStack.Create(original.DataContextType, original.Parent,
                    bindingPropertyResolvers: new Delegate[]{
                        new Func<ParsedExpressionBindingProperty, ParsedExpressionBindingProperty>(e => {
                            if (e.Expression.NodeType == ExpressionType.Constant && (string)((ConstantExpression)e.Expression).Value == "abc") return new ParsedExpressionBindingProperty(Expression.Constant("def"));
                            else return e;
                        })
                    });
            }
        }
    }
}
