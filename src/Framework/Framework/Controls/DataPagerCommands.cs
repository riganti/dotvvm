using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    public class DataPagerBindings
    {
        public ICommandBinding? GoToFirstPage { get; init; }
        public ICommandBinding? GoToPreviousPage { get; init; }
        public ICommandBinding? GoToNextPage { get; init; }
        public ICommandBinding? GoToLastPage { get; init; }
        public ICommandBinding? GoToPage { get; init; }

        public IStaticValueBinding<bool>? IsFirstPage { get; init; }
        public IStaticValueBinding<bool>? IsLastPage { get; init; }
        public IStaticValueBinding<IEnumerable<int>>? PageNumbers { get; init; }
        public IStaticValueBinding<bool>? IsActivePage { get; init; }
        public IStaticValueBinding<string>? PageNumberText { get; init; }
    }
}
