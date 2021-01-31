using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    /// <summary>
    /// Assigns <see cref="Internal.DataContextTypeProperty" /> to all controls that have different datacontext that their parent
    /// </summary>
    public class DataContextPropertyAssigningVisitor: ResolvedControlTreeVisitor
    {
        public override void VisitControl(ResolvedControl control)
        {
            if (control.DataContextTypeStack != control.Parent?.As<ResolvedControl>()?.DataContextTypeStack || control.Parent is ResolvedTreeRoot)
            {
                control.SetProperty(new ResolvedPropertyValue(Internal.DataContextTypeProperty, control.DataContextTypeStack));
            }
            base.VisitControl(control);
        }
    }
}
