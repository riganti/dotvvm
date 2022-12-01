using DotVVM.AutoUI.Annotations;

namespace DotVVM.AutoUI
{
    public class ViewContext : IViewContext
    {
        public string? ViewName { get; set; }
        public string? GroupName { get; set; }
    }
}
