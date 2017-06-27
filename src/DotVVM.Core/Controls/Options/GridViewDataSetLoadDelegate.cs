using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{

    public delegate GridViewDataSetLoadedData GridViewDataSetLoadDelegate(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public delegate GridViewDataSetLoadedData<T> GridViewDataSetLoadDelegate<T>(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public delegate Task<GridViewDataSetLoadedData> AsyncGridViewDataSetLoadDelegate(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public delegate Task<GridViewDataSetLoadedData<T>> AsyncGridViewDataSetLoadDelegate<T>(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

}
