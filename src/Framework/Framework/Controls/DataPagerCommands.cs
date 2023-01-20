using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    public class DataPagerCommands
    {
        public ICommandBinding? GoToFirstPage { get; init; }
        public ICommandBinding? GoToPreviousPage { get; init; }
        public ICommandBinding? GoToNextPage { get; init; }
        public ICommandBinding? GoToLastPage { get; init; }
        public ICommandBinding? GoToPage { get; init; }
    }
}