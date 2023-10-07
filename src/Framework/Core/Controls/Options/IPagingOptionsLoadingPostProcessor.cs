using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls;

public interface IPagingOptionsLoadingPostProcessor
{
    void ProcessLoadedItems<T>(IQueryable<T> filteredQueryable, IList<T> items);
}
