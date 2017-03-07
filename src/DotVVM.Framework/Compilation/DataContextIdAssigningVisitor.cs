using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class DataContextIdAssigningVisitor: ResolvedControlTreeVisitor
    {
        public override void VisitControl(ResolvedControl control)
        {
            if (control.DataContextTypeStack != control.Parent?.As<ResolvedControl>()?.DataContextTypeStack)
            {
                control.SetProperty(new ResolvedPropertyValue(Internal.DataContextTypeProperty, control.DataContextTypeStack));
            }
            base.VisitControl(control);
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            base.VisitPropertyBinding(propertyBinding);
        }
    }
}
