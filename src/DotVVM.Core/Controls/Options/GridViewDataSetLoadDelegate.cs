using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Controls
{

    public delegate GridViewDataSetLoadedData GridViewDataSetLoadDelegate(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

    public delegate GridViewDataSetLoadedData<T> GridViewDataSetLoadDelegate<T>(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions);

}
