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
                var markupControl = control.GetAllAncestors(includingThis: true).OfType<DotvvmMarkupControl>().First();
                var viewId = markupControl.GetDotvvmUniqueId();
                return (viewId as IValueBinding)?.GetKnockoutBindingExpression(markupControl)
                       ?? KnockoutHelper.MakeStringLiteral((string)viewId);
            }
            else
            {
                return KnockoutHelper.MakeStringLiteral(viewModuleInfo.ViewId);
            }
        }

    }
}
