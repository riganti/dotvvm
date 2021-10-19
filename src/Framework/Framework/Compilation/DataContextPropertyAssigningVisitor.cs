﻿using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
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
                var c = control.Metadata.Type;
                // RawLiteral (and similar) don't need datacontext type, it only slows down compilation
                if (c != typeof(RawLiteral) && c != typeof(GlobalizeResource) && c != typeof(RequiredResource))
                {
                    control.SetProperty(new ResolvedPropertyValue(Internal.DataContextTypeProperty, control.DataContextTypeStack));
                }
            }
            base.VisitControl(control);
        }
    }
}
