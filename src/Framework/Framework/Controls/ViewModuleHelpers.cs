using System;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    public static class ViewModuleHelpers
    {

        public static string GetViewIdJsExpression(ViewModuleReferenceInfo viewModuleInfo, DotvvmControl control)
        {
            if (viewModuleInfo.IsMarkupControl)
            {
                return "$element";
            }
            else
            {
                if (viewModuleInfo.ViewId == null)
                    throw new ArgumentException($"ViewModule's property {nameof(viewModuleInfo.ViewId)} has not been set.");

                return KnockoutHelper.MakeStringLiteral(viewModuleInfo.ViewId);
            }
        }

    }
}
