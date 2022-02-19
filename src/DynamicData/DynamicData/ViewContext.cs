using DotVVM.Framework.Controls.DynamicData.Annotations;
using System.Security.Principal;

namespace DotVVM.Framework.Controls.DynamicData
{
    public class ViewContext : IViewContext
    {
        public string? ViewName { get; set; }
        public string? GroupName { get; set; }
    }
}
