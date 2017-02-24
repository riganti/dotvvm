using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public interface INearPageIndexesProvider
    {
        IList<int> GetIndexes(IPageableGridViewDataSet dataSet);
    }
}