using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{

    public delegate GridViewDataSetLoadedData GridViewDataSetLoadDelegate(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public delegate GridViewDataSetLoadedData<T> GridViewDataSetLoadDelegate<T>(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public delegate Task<GridViewDataSetLoadedData> GridViewDataSetLoadAsyncDelegate(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public delegate Task<GridViewDataSetLoadedData<T>> GridViewDataSetLoadAsyncDelegate<T>(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

}
