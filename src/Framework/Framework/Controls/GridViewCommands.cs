using System;
using System.Collections.Concurrent;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    public class GridViewCommands
    {

        private readonly ConcurrentDictionary<string?, IValueBinding<bool>> isSortColumnAscending = new();
        private readonly ConcurrentDictionary<string?, IValueBinding<bool>> isSortColumnDescending = new();

        public ICommandBinding? SetSortExpression { get; init; }

        internal IValueBinding<Func<string, bool>>? IsColumnSortedAscending { get; init; }
        internal IValueBinding<Func<string, bool>>? IsColumnSortedDescending { get; init; }

        public IValueBinding<bool>? GetIsColumnSortedAscendingBinding(string? sortExpression)
        {
            if (IsColumnSortedAscending == null)
            {
                return null;
            }
            return isSortColumnAscending.GetOrAdd(sortExpression, _ => IsColumnSortedAscending.Select(a => a(sortExpression)));
        }

        public IValueBinding<bool>? GetIsColumnSortedDescendingBinding(string? sortExpression)
        {
            if (IsColumnSortedDescending == null)
            {
                return null;
            }
            return isSortColumnDescending.GetOrAdd(sortExpression, _ => IsColumnSortedDescending.Select(a => a(sortExpression)));
        }
    }
}
