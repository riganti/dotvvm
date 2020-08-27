using System;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Compiler
{
    public class ResolvedControlInfoVisitor: ResolvedControlTreeVisitor
    {
        public AssemblyBindingCompiler BindingCompiler { get; set; }
        public FileCompilationResult Result { get; set; }
        public DataContextStack CurrentType;

        public override void VisitView(ResolvedTreeRoot view)
        {
            CurrentType = view.DataContextTypeStack;
            base.VisitView(view);
        }

        public override void VisitControl(ResolvedControl control)
        {
            var position = control.DothtmlNode.StartPosition;
            var returnType = CurrentType;
            if (CurrentType != control.DataContextTypeStack)
            {
                CurrentType = control.DataContextTypeStack;
                Result.Controls.Add(position, new ControlCompilationInfo
                {
                    DataContext = control.DataContextTypeStack.DataContextType.ToString()
                });
            }

            base.VisitControl(control);
            CurrentType = returnType;
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            if(BindingCompiler != null)
            {
                // try to compile bindings to see errors
                try
                {
                    // T+
                    //BindingCompiler.PrecompileBinding(propertyBinding.Binding, "id123", propertyBinding.Property.PropertyType);
                }
                catch(Exception exception)
                {
                    Result.Errors.Add(exception);
                }
            }
            base.VisitPropertyBinding(propertyBinding);
        }
    }
}
