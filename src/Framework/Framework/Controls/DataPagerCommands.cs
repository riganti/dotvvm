using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    /// <summary> Contains pre-created command and value bindings for the <see cref="DataPager" /> components. An instance can be obtained from <see cref="GridViewDataSetBindingProvider" /> </summary>
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
        public IStaticValueBinding<bool>? HasMoreThanOnePage { get; init; }
    }
}
