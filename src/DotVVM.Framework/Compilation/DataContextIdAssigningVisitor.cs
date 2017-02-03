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
            if (control.DataContextTypeStack.DataContextSpaceId != control.Parent?.As<ResolvedControl>()?.DataContextTypeStack.DataContextSpaceId)
            {
                control.DataContextTypeStack.Save();
                control.SetProperty(new ResolvedPropertyValue(Internal.DataContextSpaceIdProperty, control.DataContextTypeStack.DataContextSpaceId));
            }
            base.VisitControl(control);
        }

        public override void VisitPropertyBinding(ResolvedPropertyBinding propertyBinding)
        {
            base.VisitPropertyBinding(propertyBinding);
        }
    }
}
